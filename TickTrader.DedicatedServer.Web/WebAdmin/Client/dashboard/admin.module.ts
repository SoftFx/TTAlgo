import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { OrderBy, FilterByPipe } from '../pipes/index';
import { UniversalModule } from 'angular2-universal';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NavbarModule } from '../shared/navbar/navbar.module';
import { SidebarModule } from './sidebar/sidebar.module';
import { FooterModule } from '../shared/footer/footer.module';

import { MODULE_COMPONENTS, MODULE_ROUTES } from './admin.routes';

@NgModule({
    imports: [
        UniversalModule,
        FormsModule,
        ReactiveFormsModule,
        NavbarModule,
        SidebarModule,
        FooterModule,
        RouterModule.forChild(MODULE_ROUTES)
    ],
    declarations: [
        OrderBy,
        FilterByPipe,
        MODULE_COMPONENTS,
    ]
})

export class AdminModule { }
