import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { FileUploadModule } from 'primeng/fileupload';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-upload-dialog',
  standalone: true,
  imports: [CommonModule, DialogModule, FileUploadModule, ButtonModule],
  templateUrl: 'upload-dialog.component.html',
})
export class UploadDialogComponent {
  visible = false;
  @Output() filesSelected = new EventEmitter<File[]>();

  open() {
    this.visible = true;
  }

  close() {
    this.visible = false;
  }

  onUpload(event: any) {
    const files: File[] = event.files;
    this.filesSelected.emit(files);
    this.close();
  }
}