import { Component, signal, ViewChild, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
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
export class ExplorerShellComponent implements OnInit, OnDestroy {

  @ViewChild('uploadDialog') uploadDialog!: UploadDialogComponent;
  @ViewChild(ExplorerTreeComponent) tree!: ExplorerTreeComponent;
  public DrawerMode = DrawerMode;

  isUploading = signal(false);
  pendingUploadParentId: string | null = null;
  drawerMode: DrawerMode = DrawerMode.Add;
  drawerVisible = false;
  private isProcessingFolderSelection = false;

  newObjectName = '';
  selectedFolderId: string | null = null;
  selectedFileId: string | null = null;
  private escapeKeyHandler?: (event: KeyboardEvent) => void;

  constructor(
    private folderService: FolderService,
    private fileService: FileService,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit() {

    this.folderService.loadRootFolders().subscribe({
      next: () => {

        this.selectedFolderId = null;
        this.cdr.detectChanges();

        setTimeout(() => {
          this.tree?.expandRootFolders();
        }, 200);
      },
      error: (err) => console.error('Error loading root folders:', err)
    });

    this.escapeKeyHandler = (event) => {
      if (event.key === 'Escape' && this.drawerVisible) {
        this.closeDrawer();
      }
    };

    if (typeof document !== 'undefined') {
      document.addEventListener('keydown', this.escapeKeyHandler);
    }
  }

  ngOnDestroy() {
    if (this.escapeKeyHandler && typeof document !== 'undefined') {
      document.removeEventListener('keydown', this.escapeKeyHandler);
    }
  }

  isCurrentFolderRoot(): boolean {
    return this.selectedFolderId === null;
  }

  openRenameFolderDrawer(folderId: string | null) {
    this.drawerMode = DrawerMode.RenameFolder;
    this.selectedFolderId = this.normalizeParentIdForFrontend(folderId);
    this.drawerVisible = true;
    this.cdr.detectChanges();
  }

  openRenameFileDrawer(fileId: string | null) {
    this.drawerMode = DrawerMode.RenameFile;
    this.selectedFileId = fileId ?? null;
    this.drawerVisible = true;
    this.cdr.detectChanges();
  }

  openAddFolderDrawer(folderId: string | null) {
    this.drawerMode = DrawerMode.Add;
    this.selectedFolderId = this.normalizeParentIdForFrontend(folderId);
    this.drawerVisible = true;
    this.cdr.detectChanges();
  }

  submitDrawer() {
    const name = this.newObjectName.trim();
    if (!name) return;

    switch (this.drawerMode) {
      case DrawerMode.Add:
        const parentIdFrontend = this.selectedFolderId ?? null;
        const newFolder: Partial<Folder> = { name, parentFolderId: parentIdFrontend ?? undefined }; // pass undefined for root
        console.debug('Creating folder. parentId(frontend)=', parentIdFrontend, 'name=', name);
        this.folderService.createFolder(newFolder).subscribe({
          next: (createdFolder) => {
            if (parentIdFrontend) {
              this.tree?.expandFolderById(parentIdFrontend);
            } else {
              this.folderService.refreshRootFolders().subscribe({
                next: () => {
                  console.log('Root folders reloaded after creating folder');
                  this.cdr.detectChanges();
                },
                error: err => console.error('Error reloading root folders:', err)
              });
            }
            this.closeDrawer();
          },
          error: err => console.error('Error while creating folder:', err)
        });
        break;

      case DrawerMode.RenameFolder:
        if (!this.selectedFolderId) break;
        const folder: Folder = { id: this.selectedFolderId, name } as Folder;
        this.folderService.renameFolder(folder).subscribe({
          next: () => this.closeDrawer(),
          error: err => console.error('Error while renaming folder:', err)
        });
        break;

      case DrawerMode.RenameFile:
        if (!this.selectedFileId) break;
        const file: AppFile = { id: this.selectedFileId, name, folderId: this.selectedFolderId! } as AppFile;
        this.fileService.renameFile(file).subscribe({
          next: () => this.closeDrawer(),
          error: err => console.error('Error while renaming file:', err)
        });
        break;
    }
  }

  closeDrawer() {
    this.drawerVisible = false;
    this.newObjectName = '';
    this.selectedFileId = null;
    this.cdr.detectChanges();
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
      accept: () => this.deleteFile(file.id!),
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

  openFolderById(folderId: string | null) {
    if (this.isProcessingFolderSelection) return;

    this.isProcessingFolderSelection = true;

    this.selectedFolderId = this.normalizeParentIdForFrontend(folderId);

    if (this.selectedFolderId === null) {
      console.log('ROOT folder selected');
      this.folderService.loadRootFolders().subscribe({
        next: () => {
          console.log('ROOT folder opened');
          this.selectedFolderId = null;
          this.cdr.detectChanges();
          this.isProcessingFolderSelection = false;
        },
        error: err => {
          console.error('Error while opening ROOT folder:', err);
          this.isProcessingFolderSelection = false;
        }
      });
    } else {
      this.folderService.loadFolderChildren(this.selectedFolderId).subscribe({
        next: () => {
          console.log('Folder opened');
          this.isProcessingFolderSelection = false;
        },
        error: err => {
          console.error('Error while opening folder:', err);
          this.isProcessingFolderSelection = false;
        }
      });
    }
  }

  openUploadDialog(parentId: string | null) {
    this.pendingUploadParentId = this.normalizeParentIdForFrontend(parentId);
    this.uploadDialog.open();
    this.cdr.detectChanges();
  }

  uploadFiles(ev: any) {
    if (ev && ev.files) {
      const pid = this.normalizeParentIdForFrontend(ev.parentId);
      this.uploadToServer(pid, ev.files as globalThis.File[]);
    } else if (typeof ev === 'string' || ev === null) {
      this.openUploadDialog(ev as string | null);
    }
  }

  handleFiles(files: globalThis.File[]) {
    const parentId = this.pendingUploadParentId;
    this.pendingUploadParentId = null;
    if (!files || files.length === 0) {
      this.messageService.add({ severity: 'warn', summary: 'Warning', detail: 'No files selected' });
      return;
    }
    this.uploadToServer(parentId ?? null, files);
  }

  onTreeFileSelected(ev: { fileId: string, parentId?: string | null, file?: AppFile }) {
    if (this.isProcessingFolderSelection) return;

    this.isProcessingFolderSelection = true;

    const parentNormalized = this.normalizeParentIdForFrontend(ev.parentId ?? null);
    if (parentNormalized) {
      this.selectedFolderId = parentNormalized;
      this.folderService.loadFolderChildren(parentNormalized).subscribe({
        next: () => {
          this.isProcessingFolderSelection = false;
        },
        error: err => {
          console.error('Error while loading folder on file selection:', err);
          this.isProcessingFolderSelection = false;
        }
      });
    } else {
      this.selectedFolderId = null;
      this.isProcessingFolderSelection = false;
    }
    this.selectedFileId = ev.fileId;
  }

  onFolderSelectedFromTree(folderId: string | null) {
    if (this.isProcessingFolderSelection) return;

    this.isProcessingFolderSelection = true;

    this.selectedFileId = null;
    this.selectedFolderId = this.normalizeParentIdForFrontend(folderId);

    if (this.selectedFolderId === null) {
      this.folderService.loadRootFolders().subscribe({
        next: () => {
          this.cdr.detectChanges();
          this.isProcessingFolderSelection = false;
        },
        error: (err) => {
          console.error('Error loading root folders:', err);
          this.isProcessingFolderSelection = false;
        }
      });
    } else {
      this.folderService.loadFolderChildren(this.selectedFolderId).subscribe({
        next: () => {
          this.isProcessingFolderSelection = false;
        },
        error: (err) => {
          console.error('Error loading folder children:', err);
          this.isProcessingFolderSelection = false;
        }
      });
    }
  }

  onFolderSelectedFromFiles(folderId: string) {
    this.selectedFileId = null;
    this.selectedFolderId = folderId;
    this.tree?.expandFolderById(folderId);
  }

  private uploadToServer(parentId: string | null, files: globalThis.File[]) {
    if (!files || files.length === 0) return;

    this.isUploading.set(true);

    this.fileService.uploadFiles(parentId, files)
      .pipe(finalize(() => this.isUploading.set(false)))
      .subscribe({
        next: uploadedFiles => {
          this.messageService.add({ severity: 'success', summary: 'Done', detail: `${uploadedFiles.length} file(s) uploaded` });

          if (parentId) {
            this.folderService.refreshFolderChildren(parentId).subscribe();
          } else {
            this.folderService.refreshRootFolders().subscribe({
              next: () => {
                this.cdr.detectChanges();
              },
              error: err => console.error('Error reloading root folders:', err)
            });
          }
        },
        error: err => {
          console.error('Upload failed', err);
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'File upload failed' });
        }
      });
  }

  private normalizeParentIdForFrontend(id: string | null | undefined): string | null {
    if (id === null || id === undefined) {
      return null;
    }
    const trimmed = id.trim();
    if (trimmed === '' || trimmed === '00000000-0000-0000-0000-000000000000') {
      return null;
    }
    return trimmed;
  }
}