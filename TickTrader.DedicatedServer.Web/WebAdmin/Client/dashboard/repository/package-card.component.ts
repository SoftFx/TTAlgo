import { Input, EventEmitter, Output, Component } from '@angular/core';
import { PackageModel, PluginModel } from '../../models/index';

@Component({
    selector: 'package-card-cmp',
    template: require('./package-card.component.html'),
    styles: [require('../../app.component.css')],
})

export class PackageCardComponent {

    @Input() package: PackageModel;
    @Output() onDelete = new EventEmitter<PackageModel>();

    public delete() {
        this.onDelete.emit(this.package);
    }
}