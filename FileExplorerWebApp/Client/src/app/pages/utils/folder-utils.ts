import { Injectable } from '@angular/core';
import { AppFile, Folder } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class FolderUtils {

  constructor() { }

  public deleteRecursive(folders: Folder[], targetId: string): Folder[] {
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

  public insertFilesImmutable(folders: Folder[], parentId: string, files: AppFile[]): Folder[] {
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

  public removeFileRecursive(folders: Folder[], fileId: string): Folder[] {
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

  public renameFileRecursive(folders: Folder[], fileId: string, newName: string): Folder[] {
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

  public insertChildrenImmutable(
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

  public findSubfolders(parentId: string, folders: Folder[]): Folder[] | undefined {
    for (const f of folders) {
      if (f.id === parentId) return f.subFolders;
      if (f.subFolders) {
        const found = this.findSubfolders(parentId, f.subFolders);
        if (found) return found;
      }
    }
    return undefined;
  }

  public renameRecursive(folders: Folder[], targetId: string, newName: string): Folder[] {
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
