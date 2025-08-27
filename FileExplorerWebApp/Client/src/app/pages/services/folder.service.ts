import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap, Observable, throwError } from 'rxjs';
import { AppFile, Folder } from '../models/models';

@Injectable({ providedIn: 'root' })
export class FolderService {
  private apiUrl = 'api/folder';
  folders = signal<Folder[]>([]);

  constructor(private http: HttpClient) { }

  loadRootFolders(): Observable<Folder[]> {
    return this.http.get<Folder[]>(`${this.apiUrl}/root`).pipe(
      tap(data => {
        this.folders.set(data || []);
      })
    );
  }

  getChildrenByParentId(parentId: string): Observable<Folder[]> {
    return this.http.get<Folder[]>(`${this.apiUrl}/${parentId}`);
  }

  loadFolderChildren(parentId: string): Observable<Folder[]> {
    return this.getChildrenByParentId(parentId).pipe(
      tap(response => {
        const parentDto = response[0];
        if (!parentDto) return;

        this.folders.update(current =>
          this.insertChildrenImmutable(
            current,
            parentId,
            parentDto.subFolders ?? [],
            parentDto.files ?? []
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
          this.folders.update(s => this.insertChildrenImmutable(s, created.parentFolderId!, [...(this.findSubfolders(created.parentFolderId!, s) ?? []), created]));
        }
      })
    );
  }

  renameFolder(folder: Folder): Observable<void> {
    if (!folder.id) return throwError(() => new Error('Folder id is required'));

    return this.http.put<void>(`${this.apiUrl}/${folder.id}`, folder).pipe(
      tap(() => {
        this.folders.update(current => this.renameRecursive(current, folder.id!, folder.name));
      })
    );
  }

  deleteFolder(folderId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${folderId}`).pipe(
      tap(() => {
        this.folders.update(current => this.deleteRecursive(current, folderId));
      })
    );
  }

  addFilesToFolder(parentId: string, files: AppFile[]) {
    if (!parentId || !files || files.length === 0) return;
    this.folders.update(current => this.insertFilesImmutable(current, parentId, files));
  }

  removeFileFromFolder(fileId: string) {
    if (!fileId) return;
    this.folders.update(current => this.removeFileRecursive(current, fileId));
  }

  renameFileInFolder(file: AppFile) {
    if (!file?.id) return;
    this.folders.update(current => this.renameFileRecursive(current, file.id!, file.name));
  }

  private deleteRecursive(folders: Folder[], targetId: string): Folder[] {
    return folders.reduce<Folder[]>((acc, f) => {
      if (f.id === targetId) {
        return acc;
      }

      if (f.subFolders && f.subFolders.length) {
        const newSub = this.deleteRecursive(f.subFolders, targetId);

        const updated = newSub.length !== (f.subFolders?.length ?? 0)
          ? { ...f, subFolders: newSub, hasChildren: newSub.length > 0 }
          : f;
        return [...acc, updated];
      }

      return [...acc, f];
    }, []);
  }

  private insertFilesImmutable(folders: Folder[], parentId: string, files: AppFile[]): Folder[] {
    return folders.map(f => {
      if (f.id === parentId) {
        const currentFiles = Array.isArray(f.files) ? f.files : [];
        const merged = [...currentFiles, ...files.filter(nf => !currentFiles.some(cf => cf.id === nf.id))];
        return {
          ...f,
          files: merged,
          hasChildren: (merged.length > 0) || (f.subFolders?.length ?? 0) > 0
        };
      }

      if (f.subFolders && f.subFolders.length) {
        return {
          ...f,
          subFolders: this.insertFilesImmutable(f.subFolders, parentId, files)
        };
      }

      return f;
    });
  }

  private removeFileRecursive(folders: Folder[], fileId: string): Folder[] {
    return folders.map(f => {
      let changed = false;
      let newFiles = f.files;
      if (Array.isArray(f.files) && f.files.some(x => x.id === fileId)) {
        newFiles = f.files.filter(x => x.id !== fileId);
        changed = true;
      }

      let newSub = f.subFolders;
      if (f.subFolders && f.subFolders.length) {
        const updatedSub = this.removeFileRecursive(f.subFolders, fileId);
        if (updatedSub !== f.subFolders) {
          newSub = updatedSub;
          changed = true;
        }
      }

      if (changed) {
        return {
          ...f,
          files: newFiles,
          subFolders: newSub,
          hasChildren: (newFiles?.length ?? 0) + (newSub?.length ?? 0) > 0
        };
      }

      return f;
    });
  }

  private renameFileRecursive(folders: Folder[], fileId: string, newName: string): Folder[] {
    return folders.map(f => {
      let changed = false;
      let newFiles = f.files;
      if (Array.isArray(f.files) && f.files.some(x => x.id === fileId)) {
        newFiles = f.files.map(x => x.id === fileId ? { ...x, name: newName } : x);
        changed = true;
      }

      let newSub = f.subFolders;
      if (f.subFolders && f.subFolders.length) {
        const updatedSub = this.renameFileRecursive(f.subFolders, fileId, newName);
        if (updatedSub !== f.subFolders) {
          newSub = updatedSub;
          changed = true;
        }
      }

      if (changed) {
        return { ...f, files: newFiles, subFolders: newSub };
      }
      return f;
    });
  }

  private insertChildrenImmutable(
    folders: Folder[],
    parentId: string,
    subFolders: Folder[],
    files?: any[]
  ): Folder[] {
    return folders.map(f => {
      if (f.id === parentId) {
        return {
          ...f,
          subFolders,
          files: files ?? f.files,
          hasChildren: (subFolders.length + (files?.length ?? f.files?.length ?? 0)) > 0
        };
      }

      if (f.subFolders && f.subFolders.length) {
        return {
          ...f,
          subFolders: this.insertChildrenImmutable(f.subFolders, parentId, subFolders, files)
        };
      }

      return f;
    });
  }

  private findSubfolders(parentId: string, folders: Folder[]): Folder[] | undefined {
    for (const f of folders) {
      if (f.id === parentId) return f.subFolders;
      if (f.subFolders) {
        const found = this.findSubfolders(parentId, f.subFolders);
        if (found) return found;
      }
    }
    return undefined;
  }

  private renameRecursive(folders: Folder[], targetId: string, newName: string): Folder[] {
    return folders.map(f => {
      if (f.id === targetId) {
        return { ...f, name: newName };
      }

      if (f.subFolders && f.subFolders.length) {
        const newSub = this.renameRecursive(f.subFolders, targetId, newName);

        if (newSub !== f.subFolders) {
          return { ...f, subFolders: newSub };
        }
      }

      return f;
    });
  }
}