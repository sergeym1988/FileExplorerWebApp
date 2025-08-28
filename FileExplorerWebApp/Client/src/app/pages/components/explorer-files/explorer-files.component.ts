import { Component, Input, Output, EventEmitter, OnDestroy, effect, computed } from '@angular/core';
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
export class ExplorerFilesComponent implements OnDestroy {

  private _folderId: string | null = null;
  public PreviewKind = PreviewKind;
  currentFolderParentId = computed<string | null>(() => this.findParentIdOfFolder(this.folderService.folders(), this._folderId) ?? null);
  currentFolder = computed<Folder | undefined>(() => this.findFolderById(this.folderService.folders(), this._folderId));
  folderContent = computed<(Folder | AppFile)[]>(() => {
    const folder = this.currentFolder();
    if (!folder) return [] as (Folder | AppFile)[];
    const subFolders = Array.isArray(folder.subFolders) ? folder.subFolders : [];
    const files = Array.isArray(folder.files) ? folder.files : [];
    return [...subFolders, ...files] as (Folder | AppFile)[];
  });
  @Input()
  set folderId(value: string | null) {
    this._folderId = value;
    if (value) {
      this.loadFolderContent(value);
    } else {
      this.clearPreviewCacheForAll();
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
  ) {

    effect(() => {
      if (!this._folderId) {
        this.clearPreviewCacheForAll();
      }
    });
  }

  ngOnDestroy(): void {
    this.clearPreviewCacheForAll();
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

  canGoUp(): boolean {
    return !!this.currentFolderParentId();
  }

  goUp() {
    const parentId = this.currentFolderParentId();
    if (!parentId) return;
    this.openFolder.emit(parentId);
    this._folderId = parentId;
    this.loadFolderContent(parentId);
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
      this._folderId = fid;
      this.loadFolderContent(fid);
      this.openFolder.emit(fid);
    } else if (this.isFile(item)) {
      if (item.id) this.openFile.emit(item.id);
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

      if (file.preview instanceof ArrayBuffer) {
        const bytes = new Uint8Array(file.preview as ArrayBuffer);
        const text = new TextDecoder().decode(bytes);
        return text.length > maxChars ? text.substring(0, maxChars) + '…' : text;
      }

      if (file.preview instanceof Blob) {
        return '';
      }
    } catch (e) {
      console.warn('Preview text decode failed', e);
      return '';
    }

    return '';
  }

  private clearPreviewCacheForAll() {

    for (const [id, cached] of this.previewUrlCache) {
      if (!cached) continue;
    }
    this.previewUrlCache.clear();
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
}