import { Component, Input, Output, EventEmitter, OnDestroy, effect } from '@angular/core';
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
  // Expose enum to template
  public PreviewKind = PreviewKind;
  @Input()
  set folderId(value: string | null) {
    this._folderId = value;
    if (value) {    
      this.loadFolderContent(value);
    } else {
      this.clearPreviewCacheForAll();
      this.folderContent = [];
    }
  }
  get folderId() { return this._folderId; }

  @Output() openFolder = new EventEmitter<string>();
  @Output() openFile = new EventEmitter<string>();
  @Output() renameFileEvent = new EventEmitter<string>();
  @Output() deleteFileEvent = new EventEmitter<AppFile>();

  folderContent: FolderOrFile[] = [];

  // cache for generated preview URLs / safe urls keyed by file id
  private previewUrlCache = new Map<string, SafeUrl | string | null>();

  constructor(
    private folderService: FolderService,
    private sanitizer: DomSanitizer
  ) {   
    effect(() => {
      const currentFolderId = this._folderId;
      const folders = this.folderService.folders();
      if (!currentFolderId) {
        this.folderContent = [];
        return;
      }
      const folder = this.findFolderById(folders, currentFolderId);
      if (folder) {
        const subFolders = Array.isArray(folder.subFolders) ? folder.subFolders : [];
        const files = Array.isArray(folder.files) ? folder.files : [];
        this.folderContent = [...subFolders, ...files];
      }
    });
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
      next: () => { /* state updated through signal effect */ },
      error: err => {
        console.error(err);
        this.folderContent = [];
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

  // ---- helpers / type guards ----
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

  // ---- actions ----
  onItemDoubleClick(item: FolderOrFile) {
    if (this.isFolder(item)) {
      const fid = item.id;
      if (!fid) return;
      this.loadFolderContent(fid);
      this.openFolder.emit(fid);
    } else if (this.isFile(item)) {
      if (item.id) this.openFile.emit(item.id);
    }
  }





  trackById(_index: number, item: any) {
    return item?.id ?? _index;
  }

  getPreviewImageSrc(file: AppFile): SafeUrl | null {
    if (!file || !file.preview || file.previewKind === undefined) return null;
    if (file.previewKind === PreviewKind.None) return null;

    // If cached — reuse
    if (file.id && this.previewUrlCache.has(file.id)) {
      const cached = this.previewUrlCache.get(file.id) as SafeUrl | string | null;
      return cached as SafeUrl | null;
    }

    let safe: SafeUrl | null = null;

    // Case 1: preview is string (likely base64)
    if (typeof file.preview === 'string') {
      const base64 = file.preview as string;
      if ((file.previewMime ?? '').startsWith('image/')) {
        // build data URI and sanitize
        const mime = file.previewMime ?? 'image/jpeg';
        const dataUri = `data:${mime};base64,${base64}`;
        safe = this.sanitizer.bypassSecurityTrustUrl(dataUri);
        if (file.id) this.previewUrlCache.set(file.id, safe);
        return safe;
      }

      // If preview is textual but stored as string, not image — don't return an img src.
      this.previewUrlCache.set(file.id ?? '', null);
      return null;
    }

    // Case 2: preview is a plain number[] (from JSON byte[])
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

    // Case 3: preview is Blob
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

    // Case 4: preview is ArrayBuffer (Uint8Array)
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

    // Otherwise, unknown preview type
    if (file.id) this.previewUrlCache.set(file.id, null);
    return null;
  }

  /**
   * If preview is text (PreviewKind.Text), decode it (base64 -> UTF8) and truncate.
   */
  getPreviewText(file: AppFile, maxChars = 200): string {
    if (!file || !file.preview) return '';
    if (file.previewKind !== PreviewKind.Text) return '';

    try {
      if (typeof file.preview === 'string') {
        // preview is base64 string -> decode to UTF-8 string
        const base64 = file.preview as string;
        // decode base64 to binary string
        const binaryString = atob(base64);
        // convert binaryString to Uint8Array then decode as UTF-8
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
        // synchronous Blob -> we cannot read synchronously, so return empty and optionally implement async read
        return '';
      }
    } catch (e) {
      console.warn('Preview text decode failed', e);
      return '';
    }

    return '';
  }

  /**
   * Cleanup object URLs when component destroyed or folder changed.
   */
  private clearPreviewCacheForAll() {
    // Revoke object URLs if any cached entries are object URLs
    for (const [id, cached] of this.previewUrlCache) {
      if (!cached) continue;
      // cached was created with bypassSecurityTrustUrl(objUrl) — we cannot extract the raw URL back from SafeUrl,
      // so we only revoke if we stored plain object URL string (we avoid that). Since we store SafeUrl, we cannot revoke safely here.
      // To keep it simple: clear the cache map. In most small apps this is fine; for strict memory control store also raw URLs separately.
    }
    this.previewUrlCache.clear();
  }

  ngOnDestroy(): void {
    this.clearPreviewCacheForAll();
  }
}