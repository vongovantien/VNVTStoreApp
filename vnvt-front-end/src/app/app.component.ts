import { Component } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { TranslateService, TranslateModule } from '@ngx-translate/core';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';
import { ToastComponent } from './components/toast/toast.component';
import { AppLoaderComponent } from './core/services/app-loader/app-loader.component';
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule,
    MatToolbarModule,
    MatButtonModule,
    TranslateModule,
    RouterModule,
    MainLayoutComponent,
    ToastComponent,
    AppLoaderComponent
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'vnvt-front-end';
  constructor(private translate: TranslateService) {
    this.translate.setDefaultLang('en');
    this.translate.use('en');
  }

  switchLanguage(language: string) {
    this.translate.use(language);
  }
}
