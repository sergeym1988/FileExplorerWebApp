import { Component, Input, Output, EventEmitter } from '@angular/core';
import { TableModule } from 'primeng/table';
import { CommonModule } from '@angular/common';
import { Folder, AppFile } from '../../models/models';
import { FolderService } from '../../services/folder.service';

type FolderOrFile = Folder | AppFile;

@Component({
  selector: 'explorer-files',
  standalone: true,
  imports: [TableModule, CommonModule],
  templateUrl: 'explorer-files.component.html'
})
export class ExplorerFilesComponent {

  private _folderId: string | null = null;
  @Input()
  set folderId(value: string | null) {
    this._folderId = value;
    if (value) {
      this.loadFolderContent(value);
    } else {
      this.folderContent = [];
    }
  }
  get folderId() { return this._folderId; }

  @Output() openFolder = new EventEmitter<string>();
  @Output() openFile = new EventEmitter<string>();
  @Output() renameFile = new EventEmitter<AppFile>();
  @Output() deleteFile = new EventEmitter<AppFile>();


  folderContent: FolderOrFile[] = [];


  private currentFolderId: string | null = null;

  constructor(private folderService: FolderService) { }

  private loadFolderContent(folderId: string) {

    this.currentFolderId = folderId;

    this.folderService.getChildrenByParentId(folderId).subscribe({
      next: (resp: any) => {
        const parentDto = Array.isArray(resp) ? resp[0] : resp;
        if (!parentDto) {
          this.folderContent = [];
          return;
        }

        const subFolders: Folder[] = Array.isArray(parentDto.subFolders) ? parentDto.subFolders : [];
        const files: AppFile[] = Array.isArray(parentDto.files) ? parentDto.files : [];

        this.folderContent = [...subFolders, ...files];
      },
      error: err => {
        console.error(err);
        this.folderContent = [];
      }
    });
  }

  isFile(item: any): item is AppFile {
    return !!item && typeof item === 'object' && 'mime' in item;
  }

  isFolder(item: any): item is Folder {
    return !!item && typeof item === 'object' && !('mime' in item);
  }

  getIcon(item: FolderOrFile): string {
    if (this.isFolder(item)) return 'pi pi-folder';
    const mime = (item as AppFile).mime || '';
    return mime.startsWith('image/') ? 'pi pi-image' : 'pi pi-file';
  }

  getTypeLabel(item: FolderOrFile): string {
    return this.isFolder(item) ? 'folder' : ((item as AppFile).mime || '-');
  }

  getSizeLabel(item: FolderOrFile): string {
    if (this.isFolder(item)) return '-';
    const size = (item as AppFile).size ?? 0;
    return `${(size / 1024).toFixed(1)} KB`;
  }

  onItemDoubleClick(item: FolderOrFile) {
    if (this.isFolder(item)) {
      const fid = item.id;
      if (!fid) return;
      this.currentFolderId = fid;
      this.loadFolderContent(fid);
      this.openFolder.emit(fid);
    } else if (this.isFile(item)) {
      if (item.id) this.openFile.emit(item.id);
    }
  }

  onRenameFileClicked(item: any, ev?: MouseEvent) {
    ev?.stopPropagation();
    if (this.isFile(item)) this.renameFile.emit(item);
  }

  onDeleteFileClicked(item: any, ev?: MouseEvent) {
    ev?.stopPropagation();
    if (this.isFile(item)) this.deleteFile.emit(item);
  }

  trackById(index: number, item: any) {
    return item?.id ?? index;
  }
}