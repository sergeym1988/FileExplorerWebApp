export interface Folder {
  id?: string;
  name: string;
  parentFolderId?: string;
  subFolders?: Folder[];
  files?: AppFile[];
  isExpanded?: boolean;
  hasChildren?: boolean;
}

export interface AppFile {
  id: string;
  name: string;
  mime?: string;
  size?: number;
  folderId?: string;
  content?: ArrayBuffer | Blob | string;
  preview?: ArrayBuffer | Blob | string;
  previewMime?: string;
  previewKind?: PreviewKind;
}

export enum PreviewKind {
  None = 0,
  Image = 1,
  Text = 2
}

export enum DrawerMode {
  Add,
  RenameFolder,
  RenameFile
}