import { Component, computed, EventEmitter, Output, ChangeDetectionStrategy, Input, effect, signal } from '@angular/core';
import { TreeModule } from 'primeng/tree';
import { ButtonModule } from 'primeng/button';
import { TreeNode } from 'primeng/api';
import { CommonModule } from '@angular/common';
import { FolderService } from '../../services/folder.service';
import { AppFile, Folder } from '../../models/models';

@Component({
  selector: 'explorer-tree',
  standalone: true,
  imports: [TreeModule, ButtonModule, CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrls: ['explorer-tree.component.scss'],
  templateUrl: 'explorer-tree.component.html'
})
export class ExplorerTreeComponent {
  @Input() selectedFolderId: string | null = null;
  @Output() fileSelectedEvent = new EventEmitter<{ fileId: string, parentId?: string | null, file?: AppFile }>();
  @Output() addFolderEvent = new EventEmitter<string | null>();
  @Output() renameFolderEvent = new EventEmitter<string>();
  @Output() deleteFolderEvent = new EventEmitter<Folder>();
  @Output() getFolderByIdEvent = new EventEmitter<string | null>();
  @Output() uploadFilesEvent = new EventEmitter<string | null>();

  selectedNode: TreeNode | null = null;

  nodes = computed(() => this.convertFoldersToTree(this.folderService.folders()));
  folderOnlyNodes = computed(() => this.filterOutFilesFromNodes(this.nodes()));
  private expandedKeys = signal<Set<string>>(new Set<string>());
  private isProcessingNodeSelect = false;
  private isProcessingNodeExpand = false;

  constructor(private folderService: FolderService) { }

  ngOnInit() {
    this.expandRootFolders();


    setTimeout(() => {
      this.expandRootFolders();
    }, 100);
  }

  expandRootFolders() {
    const rootFolders = this.folderService.folders();
    if (rootFolders && rootFolders.length > 0) {
      rootFolders.forEach(rootFolder => {
        this.expandFolderById(rootFolder.id);
      });
    }
  }

  addFolder(node: TreeNode) {
    const id = node.key as string | undefined;
    const parent = this.isRootFolder(node) ? null : (id ?? null);
    this.addFolderEvent.emit(parent);
  }

  renameFolder(node: TreeNode) { this.renameFolderEvent.emit(node.key as string); }

  deleteFolder(node: TreeNode) { this.deleteFolderEvent.emit(node.data.folder as Folder); }

  uploadFile(node: TreeNode) {
    const id = node.key as string | undefined;
    const parent = this.isRootFolder(node) ? null : (id ?? null);
    this.uploadFilesEvent.emit(parent);
  }

  trackByNode(node: TreeNode) {
    return node.key;
  }

  onNodeSelect(event: any) {
    if (this.isProcessingNodeSelect) return;

    this.isProcessingNodeSelect = true;

    const node: TreeNode = event.node;

    if (node.data?.type === 'folder') {
      if (this.isRootFolder(node)) {
        this.getFolderByIdEvent.emit(null); // Emit null for ROOT
      } else {
        this.getFolderByIdEvent.emit(node.key as string);
      }
    } else if (node.data?.type === 'file') {
      const file: AppFile | undefined = node.data?.file;
      this.fileSelectedEvent.emit({ fileId: node.key as string, parentId: file?.folderId ?? null, file });
    }
    this.selectedNode = node;

    setTimeout(() => {
      this.isProcessingNodeSelect = false;
    }, 300);
  }

  onNodeExpand(event: any) {
    if (this.isProcessingNodeExpand) return;

    this.isProcessingNodeExpand = true;

    const node: TreeNode = event.node;
    const id = node.key as string;
    if (id) {
      const currentExpanded = this.expandedKeys();
      currentExpanded.add(id);
      this.expandedKeys.set(new Set(currentExpanded));
    }

    this.isProcessingNodeExpand = false;
  }

  onNodeCollapse(event: any) {
    const id = event.node.key as string;
    if (id) {
      const currentExpanded = this.expandedKeys();
      currentExpanded.delete(id);
      this.expandedKeys.set(new Set(currentExpanded));
    }
  }

  expandFolderById(folderId: string) {
    if (!folderId) return;
    const currentExpanded = this.expandedKeys();
    currentExpanded.add(folderId);
    this.expandedKeys.set(new Set(currentExpanded));
    const folder = this.findFolderById(this.folderService.folders(), folderId);
    if (folder && (!folder.subFolders || folder.subFolders.length === 0)) {
      this.folderService.loadSubFolders(folderId).subscribe({
        error: err => console.error('loadSubFolders error:', err)
      });
    }
  }

  isRootFolder(node: TreeNode): boolean {
    const nodeId = node.key as string;
    const rootFolders = this.folderService.folders();
    const isInRootFolders = rootFolders && rootFolders.some(rf => rf.id === nodeId);
    return isInRootFolders;
  }

  private findFolderById(folders: Folder[] | undefined, id: string): Folder | undefined {
    if (!folders || !id) return undefined;
    for (const f of folders) {
      if (f.id === id) return f;
      if (f.subFolders) {
        const found = this.findFolderById(f.subFolders, id);
        if (found) return found;
      }
    }
    return undefined;
  }

  private buildChildren(folder: Folder): TreeNode[] | undefined {
    const subFolders = folder.subFolders ?? null;
    const files = folder.files ?? null;

    if (Array.isArray(subFolders) || Array.isArray(files)) {
      const folderChildren: TreeNode[] = (subFolders ?? []).map(sf => this.toTreeNode(sf));
      const fileChildren: TreeNode[] = (files ?? []).map(f => ({
        key: f.id ?? '',
        label: f.name,
        data: { type: 'file', file: f },
        leaf: true
      } as TreeNode));

      return [...folderChildren, ...fileChildren];
    }

    return folder.hasChildren ? undefined : [];
  }

  private convertFoldersToTree(folders: Folder[] | undefined): TreeNode[] {
    if (!folders || folders.length === 0) return [];
    return folders.map(f => this.toTreeNode(f));
  }

  private toTreeNode(folder: Folder): TreeNode {
    const id = folder.id ?? '';

    return {
      key: id,
      label: folder.name,
      data: { type: 'folder', folder },
      children: this.buildChildren(folder),
      leaf: !folder.hasChildren,
      expanded: id ? this.expandedKeys().has(id) : false
    } as TreeNode;
  }

  private filterOutFilesFromNodes(nodes: TreeNode[] | undefined): TreeNode[] {
    if (!nodes || nodes.length === 0) return [];

    return nodes
      .filter(n => n?.data?.type === 'folder')
      .map(n => {
        const clone: TreeNode = {
          ...n,
          children: n.children ? this.filterOutFilesFromNodes(n.children) : undefined
        } as TreeNode;

        if (clone.children && clone.children.length === 0) {
          clone.children = undefined;
        }
        return clone;
      });
  }
}