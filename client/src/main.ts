/// <reference types="@angular/localize" />

import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { AppModule } from './app/content/app.module';



platformBrowserDynamic().bootstrapModule(AppModule)
  .catch(err => console.error(err));
