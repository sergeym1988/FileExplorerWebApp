import { Component, signal, ViewChild, OnInit, ChangeDetectorRef } from '@angular/core';
import { SplitterModule } from 'primeng/splitter';
import { DrawerModule } from 'primeng/drawer';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ExplorerTreeComponent } from '../explorer-tree/explorer-tree.component';
import { ExplorerFilesComponent } from '../explorer-files/explorer-files.component';
import { FolderService } from '../../services/folder.service';
import { AppFile, DrawerMode, Folder } from '../../models/models';
import { ConfirmationService, MessageService } from 'primeng/api';
import { UploadDialogComponent } from '../upload-dialog/upload-dialog.component';
import { FileService } from '../../services/file.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'explorer-shell',
  standalone: true,
  imports: [
    SplitterModule,
    DrawerModule,
    ButtonModule,
    InputTextModule,
    ConfirmDialogModule,
    FormsModule,
    CommonModule,
    ExplorerTreeComponent,
    ExplorerFilesComponent,
    UploadDialogComponent
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: 'explorer-shell.component.html',
})
export class ExplorerShellComponent implements OnInit {

  @ViewChild('uploadDialog') uploadDialog!: UploadDialogComponent;
  @ViewChild(ExplorerTreeComponent) tree!: ExplorerTreeComponent;
  public DrawerMode = DrawerMode;

  isUploading = signal(false);
  pendingUploadParentId: string | null = null;
  drawerMode: DrawerMode = DrawerMode.Add;
  drawerVisible = false;

  newObjectName = '';
  selectedFolderId: string | null = null;
  selectedFileId: string | null = null;

  constructor(
    private folderService: FolderService,
    private fileService: FileService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {
    this.folderService.loadRootFolders().subscribe({
      next: (rootFolders) => {
        if (rootFolders && rootFolders.length > 0) {
          const firstRootFolder = rootFolders[0];
          Promise.resolve().then(() => {
            this.selectedFolderId = firstRootFolder.id;
            this.cdr.detectChanges();
          });
        }
      },
      error: (err) => console.error('Error loading root folders:', err)
    });
  }

  openAddFolderDrawer(folderId: string) {
    this.drawerMode = DrawerMode.Add;
    this.selectedFolderId = folderId;
    this.drawerVisible = true;
  }

  openRenameFolderDrawer(folderId: string) {
    this.drawerMode = DrawerMode.RenameFolder;
    this.selectedFolderId = folderId;
    this.drawerVisible = true;
  }

  openRenameFileDrawer(fileId: string) {
    this.drawerMode = DrawerMode.RenameFile;
    this.selectedFileId = fileId;
    this.drawerVisible = true;
  }

  submitDrawer() {
    const name = this.newObjectName.trim();
    if (!name) return;

    if (this.drawerMode === DrawerMode.Add && this.selectedFolderId) {
      const newFolder: Partial<Folder> = { name, parentFolderId: this.selectedFolderId };
      this.folderService.createFolder(newFolder).subscribe({
        next: () => {
          this.tree?.expandFolderById(this.selectedFolderId!);
          this.closeDrawer();
        },
        error: err => console.error('Error while creating folder:', err)
      });
    }

    if (this.drawerMode === DrawerMode.RenameFolder && this.selectedFolderId) {
      const folder: Folder = { id: this.selectedFolderId, name } as Folder;
      this.folderService.renameFolder(folder).subscribe({
        next: () => this.closeDrawer(),
        error: err => console.error('Error while renaming folder:', err)
      });
    }

    if (this.drawerMode === DrawerMode.RenameFile && this.selectedFileId) {
      const file: AppFile = { id: this.selectedFileId, name, folderId: this.selectedFolderId! } as AppFile;
      this.fileService.renameFile(file).subscribe({
        next: () => this.closeDrawer(),
        error: err => console.error('Error while renaming file:', err)
      });
    }
  }

  closeDrawer() {
    this.drawerVisible = false;
    this.newObjectName = '';
    this.selectedFileId = null;
  }

  openConfirmationDialog(folder: Folder) {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete folder "${folder.name}"?`,
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Confirm',
      rejectLabel: 'Cancel',
      accept: () => this.deleteFolder(folder.id ?? ''),
      reject: () => console.log('Folder deletion cancelled')
    });
  }

  openConfirmationDialogForFile(file: AppFile) {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete file "${file.name}"?`,
      header: 'Confirmation',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Confirm',
      rejectLabel: 'Cancel',
      accept: () => this.deleteFile(file.id),
      reject: () => console.log('File deletion cancelled')
    });
  }

  deleteFolder(folderId: string) {
    this.folderService.deleteFolder(folderId).subscribe({
      next: () => console.log('Folder deleted'),
      error: err => console.error('Error while deleting folder:', err)
    });
  }

  deleteFile(fileId: string) {
    this.fileService.deleteFile(fileId).subscribe({
      next: () => console.log('File deleted'),
      error: err => console.error('Error while deleting file:', err)
    });
  }

  openFolderById(folderId: string) {
    this.selectedFolderId = folderId;
    this.folderService.loadFolderChildren(folderId).subscribe({
      next: () => console.log('Folder opened'),
      error: err => console.error('Error while opening folder:', err)
    });
  }

  openUploadDialog(parentId: string) {
    this.pendingUploadParentId = parentId;
    this.uploadDialog.open();
  }

  uploadFiles(ev: any) {
    if (ev && ev.files) {
      this.uploadToServer(ev.parentId, ev.files as globalThis.File[]);
    } else if (typeof ev === 'string') {
      this.openUploadDialog(ev);
    }
  }

  handleFiles(files: globalThis.File[]) {
    const parentId = this.pendingUploadParentId;
    this.pendingUploadParentId = null;
    if (!parentId) {
      this.messageService.add({ severity: 'warn', summary: 'Warning', detail: 'No target folder selected for upload.' });
      return;
    }
    this.uploadToServer(parentId, files);
  }

  onTreeFileSelected(ev: { fileId: string, parentId?: string, file?: AppFile }) {
    if (ev.parentId) {
      this.selectedFolderId = ev.parentId;
      this.folderService.loadFolderChildren(ev.parentId).subscribe({
        next: () => { },
        error: err => console.error('Error while loading folder on file selection:', err)
      });
    }
    this.selectedFileId = ev.fileId;
  }

  onFolderSelectedFromTree(folderId: string) {
    this.selectedFileId = null;
    this.selectedFolderId = folderId;
    this.folderService.loadFolderChildren(folderId).subscribe();
  }

  onFolderSelectedFromFiles(folderId: string) {
    this.selectedFileId = null;
    this.selectedFolderId = folderId;
    this.tree?.expandFolderById(folderId);
  }

  private uploadToServer(parentId: string, files: globalThis.File[]) {
    if (!files || files.length === 0) return;

    this.isUploading.set(true);

    this.fileService.uploadFiles(parentId, files)
      .pipe(finalize(() => this.isUploading.set(false)))
      .subscribe({
        next: uploadedFiles => {
          this.messageService.add({ severity: 'success', summary: 'Done', detail: `${uploadedFiles.length} file(s) uploaded` });
        },
        error: err => {
          console.error('Upload failed', err);
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'File upload failed' });
        }
      });
  }
}