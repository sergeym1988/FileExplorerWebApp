import { Component, Input, Output, EventEmitter, OnDestroy, OnInit, effect, computed } from '@angular/core';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CommonModule } from '@angular/common';
import { Folder, AppFile, PreviewKind } from '../../models/models';
import { FolderService } from '../../services/folder.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

type FolderOrFile = Folder | AppFile;

@Component({
  selector: 'explorer-files',
  standalone: true,
  imports: [TableModule, ButtonModule, CommonModule],
  templateUrl: 'explorer-files.component.html',
  styleUrls: ['explorer-files.component.scss']
})
export class ExplorerFilesComponent implements OnDestroy, OnInit {

  private _folderId: string | null = null;
  public PreviewKind = PreviewKind;
  currentFolderParentId = computed<string | null>(() => {   
    if (this._folderId === null) {
      return null;
    }
    return this.findParentIdOfFolder(this.folderService.folders(), this._folderId) ?? null;
  });
  currentFolder = computed<Folder | undefined>(() => this.findFolderById(this.folderService.folders(), this._folderId));
  folderContent = computed<(Folder | AppFile)[]>(() => {  
    if (this._folderId === null) {
      const rootFolders = this.folderService.folders();
      if (!rootFolders || rootFolders.length === 0) return [];

      // Collect all files and folders from root folders
      const allItems: (Folder | AppFile)[] = [];
      rootFolders.forEach(rootFolder => {
        // Add subfolders if they exist
        if (Array.isArray(rootFolder.subFolders)) {
          allItems.push(...rootFolder.subFolders);
        }

        // Add files if they exist
        if (Array.isArray(rootFolder.files)) {
          allItems.push(...rootFolder.files);
        }
      });

      return allItems;
    }

    const folder = this.currentFolder();
    if (!folder) return [] as (Folder | AppFile)[];
    const subFolders = Array.isArray(folder.subFolders) ? folder.subFolders : [];
    const files = Array.isArray(folder.files) ? folder.files : [];
    return [...subFolders, ...files] as (Folder | AppFile)[];
  });

  @Input()
  set folderId(value: string | null) {
    this._folderId = value;
   
    this.clearPreviewCacheForAll();

    if (!value) {
      return;
    }

    const folder = this.findFolderById(this.folderService.folders(), value);

    const rootFolders = this.folderService.folders();
    const isRootFolder = rootFolders && rootFolders.some(rf => rf.id === value);

    if (!folder || !folder.subFolders || !folder.files) {
      if (!isRootFolder) {
        this.loadFolderContent(value);
      }
    }
  }
  get folderId() { return this._folderId; }

  @Output() openFolder = new EventEmitter<string>();
  @Output() openFile = new EventEmitter<string>();
  @Output() renameFileEvent = new EventEmitter<string>();
  @Output() deleteFileEvent = new EventEmitter<AppFile>();


  private previewUrlCache = new Map<string, SafeUrl | string | null>();

  constructor(
    private folderService: FolderService,
    private sanitizer: DomSanitizer
  ) { }

  ngOnInit() {  
  }

  onRenameFileClicked(file: AppFile, ev?: MouseEvent) {
    ev?.stopPropagation();
    if (!file || !file.id) return;
    this.renameFileEvent.emit(file.id);
  }

  onDeleteFileClicked(file: AppFile, ev?: MouseEvent) {
    ev?.stopPropagation();
    if (!file) return;
    this.deleteFileEvent.emit(file);
  }



  private loadFolderContent(folderId: string) {
    this.clearPreviewCacheForAll();
    this.folderService.loadFolderChildren(folderId).subscribe({
      next: () => { },
      error: err => {
        console.error(err);
      }
    });
  }

  private findFolderById(folders: Folder[] | undefined, id: string | null | undefined): Folder | undefined {
    if (!folders || !id) return undefined;
    for (const f of folders) {
      if (f.id === id) return f;
      const found = this.findFolderById(f.subFolders, id);
      if (found) return found;
    }
    return undefined;
  }

  private findParentIdOfFolder(folders: Folder[] | undefined, childId: string | null | undefined): string | undefined {
    if (!folders || !childId) return undefined;
    for (const f of folders) {
      if (Array.isArray(f.subFolders) && f.subFolders.some(sf => sf.id === childId)) {
        return f.id;
      }
      const found = this.findParentIdOfFolder(f.subFolders, childId);
      if (found) return found;
    }
    return undefined;
  }

  canGoUp(): boolean {  
    if (this._folderId === null) {
      return false;
    }
    return !!this.currentFolderParentId();
  }

  goUp() {
    const parentId = this.currentFolderParentId();
    if (!parentId) return;

    this._folderId = parentId;
    this.loadFolderContent(parentId);
    this.openFolder.emit(parentId);
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
    if (this.isFolder(item) && item.id) {
      this._folderId = item.id;
      this.loadFolderContent(item.id);
      this.openFolder.emit(item.id);
    } else if (this.isFile(item) && item.id) {
      this.openFile.emit(item.id);
    }
  }

  getPreviewImageSrc(file: AppFile): SafeUrl | null {
    if (!file || !file.preview || file.previewKind === undefined) return null;
    if (file.previewKind === PreviewKind.None) return null;

    if (file.id && this.previewUrlCache.has(file.id)) {
      const cached = this.previewUrlCache.get(file.id) as SafeUrl | string | null;
      return cached as SafeUrl | null;
    }

    let safe: SafeUrl | null = null;

    if (typeof file.preview === 'string') {
      const base64 = file.preview as string;
      if ((file.previewMime ?? '').startsWith('image/')) {
        const mime = file.previewMime ?? 'image/jpeg';
        const dataUri = `data:${mime};base64,${base64}`;
        safe = this.sanitizer.bypassSecurityTrustUrl(dataUri);
        if (file.id) this.previewUrlCache.set(file.id, safe);
        return safe;
      }

      this.previewUrlCache.set(file.id ?? '', null);
      return null;
    }
    if (Array.isArray(file.preview)) {
      try {
        const uint8 = new Uint8Array(file.preview as number[]);
        const blob = new Blob([uint8], { type: file.previewMime ?? file.mime ?? 'application/octet-stream' });
        const objUrl = URL.createObjectURL(blob);
        safe = this.sanitizer.bypassSecurityTrustUrl(objUrl);
        if (file.id) this.previewUrlCache.set(file.id, safe);
        return safe;
      } catch {
        this.previewUrlCache.set(file.id ?? '', null);
        return null;
      }
    }

    if (file.preview instanceof Blob) {
      try {
        const objUrl = URL.createObjectURL(file.preview);
        safe = this.sanitizer.bypassSecurityTrustUrl(objUrl);
        if (file.id) this.previewUrlCache.set(file.id, safe);
        return safe;
      } catch {
        this.previewUrlCache.set(file.id ?? '', null);
        return null;
      }
    }

    if (file.preview instanceof ArrayBuffer) {
      try {
        const arrayBuffer = file.preview as ArrayBuffer;
        const blob = new Blob([new Uint8Array(arrayBuffer)], { type: file.previewMime ?? file.mime ?? 'application/octet-stream' });
        const objUrl = URL.createObjectURL(blob);
        safe = this.sanitizer.bypassSecurityTrustUrl(objUrl);
        if (file.id) this.previewUrlCache.set(file.id, safe);
        return safe;
      } catch {
        this.previewUrlCache.set(file.id ?? '', null);
        return null;
      }
    }

    if (file.id) this.previewUrlCache.set(file.id, null);
    return null;
  }

  getPreviewText(file: AppFile, maxChars = 200): string {
    if (!file || !file.preview) return '';
    if (file.previewKind !== PreviewKind.Text) return '';

    try {
      if (typeof file.preview === 'string') {
        const base64 = file.preview as string;
        const binaryString = atob(base64);
        const len = binaryString.length;
        const bytes = new Uint8Array(len);
        for (let i = 0; i < len; i++) bytes[i] = binaryString.charCodeAt(i);
        const text = new TextDecoder().decode(bytes);
        return text.length > maxChars ? text.substring(0, maxChars) + '…' : text;
      }
      return '';
    } catch {
      return '';
    }
  }

  private clearPreviewCacheForAll() {
    this.previewUrlCache.forEach(value => {
      if (typeof value === 'string' && value.startsWith('blob:')) {
        URL.revokeObjectURL(value);
      }
    });
    this.previewUrlCache.clear();
  }

  ngOnDestroy() {
    this.clearPreviewCacheForAll();
  }
}