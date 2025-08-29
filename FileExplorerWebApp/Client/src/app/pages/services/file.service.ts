import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, throwError } from 'rxjs';
import { AppFile as AppFile } from '../models/models';
import { FolderService } from './folder.service';

@Injectable({ providedIn: 'root' })
export class FileService {
  private apiUrl = 'api/files';
  files = signal<AppFile[]>([]);

  constructor(private http: HttpClient, private folderService: FolderService) { }

  uploadFiles(parentId: string, files: globalThis.File[]): Observable<AppFile[]> {
    const formData = new FormData();
    formData.append('parentId', parentId);
    files.forEach((f: globalThis.File) => formData.append('files', f, f.name));

    return this.http.post<AppFile[]>(`${this.apiUrl}`, formData).pipe(
      tap(uploaded => {
        this.files.update(current => [...current, ...uploaded]);

        this.folderService.addFilesToFolder(parentId, uploaded);
      })
    );
  }

  deleteFile(fileId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${fileId}`).pipe(
      tap(() => {
        this.files.update(current => current.filter(f => f.id !== fileId));

        this.folderService.removeFileFromFolder(fileId);
      })
    );
  }

  renameFile(file: AppFile): Observable<void> {
    if (!file.id) return throwError(() => new Error('File id is required'));

    return this.http.patch<void>(`${this.apiUrl}/${file.id}`, file).pipe(
      tap(() => {
        this.files.update(current =>
          current.map(f => f.id === file.id ? { ...f, name: file.name } : f)
        );

        this.folderService.renameFileInFolder(file);
      })
    );
  }
}