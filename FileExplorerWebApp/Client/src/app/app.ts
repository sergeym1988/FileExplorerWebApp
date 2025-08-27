import { Component } from '@angular/core';
import { ExplorerShellComponent } from './pages/components/explorer-shell/explorer-shell.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [ExplorerShellComponent],
  template: `<explorer-shell></explorer-shell>`,
})
export class App { }