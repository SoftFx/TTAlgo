import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'admin-cmp',
    template: require('./admin.component.html'),
    styles: [require('../app.component.css')],
})

export class AdminComponent implements OnInit {
    ngOnInit() {
        $.getScript('/assets/js/gui.js');
    }
}
