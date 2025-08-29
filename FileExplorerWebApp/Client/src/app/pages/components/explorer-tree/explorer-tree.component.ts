import { Component, computed, EventEmitter, Output, ChangeDetectionStrategy, Input, effect } from '@angular/core';
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
  @Output() fileSelectedEvent = new EventEmitter<{ fileId: string, parentId?: string, file?: AppFile }>();
  @Output() addFolderEvent = new EventEmitter<string>();
  @Output() renameFolderEvent = new EventEmitter<string>();
  @Output() deleteFolderEvent = new EventEmitter<Folder>();
  @Output() getFolderByIdEvent = new EventEmitter<string>();
  @Output() uploadFilesEvent = new EventEmitter<string>();


  selectedNode: TreeNode | null = null;

  nodes = computed(() => this.convertFoldersToTree(this.folderService.folders()));
  folderOnlyNodes = computed(() => this.filterOutFilesFromNodes(this.nodes()));

  private expandedKeys = new Set<string>();

  constructor(private folderService: FolderService) {
    effect(() => {
      const rootFolders = this.folderService.folders();
      if (rootFolders && rootFolders.length > 0) {
        const firstRootFolder = rootFolders[0];
        Promise.resolve().then(() => {
          this.expandedKeys.add(firstRootFolder.id);
        });
      }
    });
  }

  ngOnInit() {
  }

  addFolder(node: TreeNode) { this.addFolderEvent.emit(node.key as string); }
  renameFolder(node: TreeNode) { this.renameFolderEvent.emit(node.key as string); }
  deleteFolder(node: TreeNode) { this.deleteFolderEvent.emit(node.data.folder as Folder); }
  uploadFile(node: TreeNode) { this.uploadFilesEvent.emit(node.key as string); }

  trackByNode(node: TreeNode) {
    return node.key;
  }

  onLabelClick(node: TreeNode) {
    if (node.data?.type === 'folder') {
      this.getFolderByIdEvent.emit(node.key as string);
    } else if (node.data?.type === 'file') {
      const file: AppFile | undefined = node.data?.file;
      this.fileSelectedEvent.emit({ fileId: node.key as string, parentId: file?.folderId, file });
    }
    this.selectedNode = node;
  }

  onNodeSelect(event: any) {
    const node: TreeNode = event.node;
    if (node.data?.type === 'folder') {
      this.getFolderByIdEvent.emit(node.key as string);
    } else if (node.data?.type === 'file') {
      const file: AppFile | undefined = node.data?.file;
      this.fileSelectedEvent.emit({ fileId: node.key as string, parentId: file?.folderId, file });
    }
    this.selectedNode = node;
  }

  onNodeExpand(event: any) {
    const node: TreeNode = event.node;
    const id = node.key as string;
    if (id) this.expandedKeys.add(id);

    if (!node.children || node.children.length === 0) {
      this.folderService.loadSubFolders(id).subscribe();
    }
  }

  onNodeCollapse(event: any) {
    const id = event.node.key as string;
    if (id) this.expandedKeys.delete(id);
  }

  expandFolderById(folderId: string) {
    if (!folderId) return;
    this.expandedKeys.add(folderId);
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
      expanded: id ? this.expandedKeys.has(id) : false
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