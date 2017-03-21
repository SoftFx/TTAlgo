import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MasonryModule } from 'angular2-masonry';
import { OrderByPipe, FilterByPipe, ResourcePipe } from '../pipes/index';
import { FileModelDirective } from '../directives/base64-file-input.directive';
import { UniversalModule } from 'angular2-universal';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NavbarModule } from '../shared/navbar/navbar.module';
import { SidebarModule } from '../shared/sidebar/sidebar.module';
import { FooterModule } from '../shared/footer/footer.module';
import { OverlayComponent } from '../shared/overlay.component';

import { MODULE_COMPONENTS, MODULE_ROUTES } from './admin.routes';

@NgModule({
    imports: [
        UniversalModule,
        MasonryModule,
        FormsModule,
        ReactiveFormsModule,
        NavbarModule,
        SidebarModule,
        FooterModule,
        RouterModule.forChild(MODULE_ROUTES)
    ],
    declarations: [
        OrderByPipe,
        FilterByPipe,
        ResourcePipe,
        OverlayComponent,
        FileModelDirective,
        MODULE_COMPONENTS,
    ]
})

export class AdminModule { }
