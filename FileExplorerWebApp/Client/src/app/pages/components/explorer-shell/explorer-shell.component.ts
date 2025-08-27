import { Component, signal, ViewChild } from '@angular/core';
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
export class ExplorerShellComponent {

  @ViewChild('uploadDialog') uploadDialog!: UploadDialogComponent;
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
    private messageService: MessageService
  ) { }

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
      const newFolder: Folder = { name, parentFolderId: this.selectedFolderId };
      this.folderService.createFolder(newFolder).subscribe({
        next: () => this.closeDrawer(),
        error: err => console.error('Error while creating folder:', err)
      });
    }

    if (this.drawerMode === DrawerMode.RenameFolder && this.selectedFolderId) {
      const folder: Folder = { id: this.selectedFolderId, name };
      this.folderService.renameFolder(folder).subscribe({
        next: () => this.closeDrawer(),
        error: err => console.error('Error while renaming folder:', err)
      });
    }

    if (this.drawerMode === DrawerMode.RenameFile && this.selectedFileId) {
      const file: AppFile = { id: this.selectedFileId, name };
      this.fileService.renameFile(file).subscribe({
        next: () => this.closeDrawer(),
        error: err => console.error('Error while renaming file:', err)
      });
    }
  }

  closeDrawer() {
    this.drawerVisible = false;
    this.newObjectName = '';
    this.selectedFolderId = null;
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
    console.log('deleteFile', fileId);
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

  openUploadDialogFor(parentId: string) {
    this.pendingUploadParentId = parentId;
    this.uploadDialog.open();
  }

  uploadFiles(ev: any) {
    if (ev && ev.files) {
      this.uploadToServer(ev.parentId, ev.files as globalThis.File[]);
    } else if (typeof ev === 'string') {
      this.openUploadDialogFor(ev);
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
        next: () => { /* ok */ },
        error: err => console.error('Error while loading folder on file selection:', err)
      });
    }
    this.selectedFileId = ev.fileId;
  }

  onFolderSelectedFromTree(folderId: string) {
    this.selectedFileId = null;
    this.selectedFolderId = folderId;
  }

  onFolderSelectedFromFiles(folderId: string) {
    this.selectedFileId = null;
    this.selectedFolderId = folderId;
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