import { Component, OnInit } from '@angular/core';

//Hack - make sure that jQuery plugins can find.
//Need to find a better solution
import * as $ from 'jquery';
window["$"] = $;
window["jQuery"] = $;


@Component({
    selector: 'admin-cmp',
    template: require('./admin.component.html'),
    styles: [require('../app.component.css')],
})

export class AdminComponent implements OnInit{
    ngOnInit() {
        $.getScript('/assets/js/gui.js');
    }
}
