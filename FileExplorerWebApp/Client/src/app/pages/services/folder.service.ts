import { Injectable, signal, PLATFORM_ID, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap, Observable, throwError } from 'rxjs';
import { AppFile, Folder } from '../models/models';
import { FolderUtils } from '../utils/folder-utils';
import { isPlatformBrowser } from '@angular/common';

@Injectable({ providedIn: 'root' })
export class FolderService {
  private apiUrl = 'api/folders';
  folders = signal<Folder[]>([]);

  constructor(
    private http: HttpClient,
    private folderUtils: FolderUtils,
    @Inject(PLATFORM_ID) private platformId: Object
  ) { }

  loadRootFolders(): Observable<Folder[]> {

    return this.http.get<Folder[]>(`${this.apiUrl}/root`).pipe(
      tap(data => {
        this.folders.set(data || []);
      })
    );
  }

  getContentByParentFolderId(parentId: string): Observable<Folder[]> {

    return this.http.get<Folder[]>(`${this.apiUrl}/${parentId}/content`);
  }

  getSubFoldersByParentId(parentId: string): Observable<Folder[]> {

    return this.http.get<Folder[]>(`${this.apiUrl}/${parentId}/subfolders`);
  }

  loadFolderChildren(parentId: string): Observable<Folder[]> {

    return this.getContentByParentFolderId(parentId).pipe(
      tap(response => {
        const parentDto = response[0];
        if (!parentDto) return;

        this.folders.update(current =>
          this.folderUtils.insertChildrenImmutable(
            current,
            parentId,
            parentDto.subFolders ?? [],
            parentDto.files ?? []
          )
        );
      })
    );
  }

  loadSubFolders(parentId: string): Observable<Folder[]> {
    if (!isPlatformBrowser(this.platformId)) {
      return new Observable(subscriber => {
        subscriber.next([]);
        subscriber.complete();
      });
    }

    return this.getSubFoldersByParentId(parentId).pipe(
      tap(response => {
        const parentDto = response[0];
        if (!parentDto) return;

        this.folders.update(current =>
          this.folderUtils.insertChildrenImmutable(
            current,
            parentId,
            parentDto.subFolders ?? [],
            undefined
          )
        );
      })
    );
  }

  createFolder(folder: Partial<Folder>) {

    return this.http.post<Folder>(this.apiUrl, folder).pipe(
      tap(created => {
        if (!created.parentFolderId) {
          this.folders.update(s => [...s, created]);
        } else {
          this.folders.update(s => this.folderUtils.insertChildrenImmutable(s, created.parentFolderId!, [...(this.folderUtils.findSubfolders(created.parentFolderId!, s) ?? []), created]));
        }
      })
    );
  }

  renameFolder(folder: Folder): Observable<void> {
    if (!folder.id) return throwError(() => new Error('Folder id is required'));

    return this.http.patch<void>(`${this.apiUrl}/${folder.id}`, folder).pipe(
      tap(() => {
        this.folders.update(current => this.folderUtils.renameRecursive(current, folder.id!, folder.name));
      })
    );
  }

  deleteFolder(folderId: string): Observable<void> {

    return this.http.delete<void>(`${this.apiUrl}/${folderId}`).pipe(
      tap(() => {
        this.folders.update(current => this.folderUtils.deleteRecursive(current, folderId));
      })
    );
  }

  addFilesToFolder(parentId: string | null, files: AppFile[]) {
    if (!parentId || !files || files.length === 0) return;
    this.folders.update(current => this.folderUtils.insertFilesImmutable(current, parentId, files));
  }

  removeFileFromFolder(fileId: string) {
    if (!fileId) return;
    this.folders.update(current => this.folderUtils.removeFileRecursive(current, fileId));
  }

  renameFileInFolder(file: AppFile) {
    if (!file?.id) return;
    this.folders.update(current => this.folderUtils.renameFileRecursive(current, file.id!, file.name));
  }

  refreshRootFolders(): Observable<Folder[]> {

    return this.loadRootFolders();
  }

  refreshFolderChildren(parentId: string): Observable<Folder[]> {

    return this.loadFolderChildren(parentId);
  }
}