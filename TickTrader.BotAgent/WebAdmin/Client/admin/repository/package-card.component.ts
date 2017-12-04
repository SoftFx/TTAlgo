import { Input, EventEmitter, Output, Component } from '@angular/core';
import { PackageModel, PluginModel } from '../../models/index';

@Component({
    selector: 'package-card-cmp',
    template: require('./package-card.component.html'),
    styles: [require('../../app.component.css')],
})

export class PackageCardComponent {

    public ConfirmDeletionEnabled: boolean;

    @Input() Package: PackageModel;
    @Output() OnDeleted = new EventEmitter<PackageModel>();

    public InitDeletion() {
        this.ConfirmDeletionEnabled = true;
    }

    public DeletionCanceled() {
        this.ConfirmDeletionEnabled = false;
    }

    public DeletionCompleted(packageModel: PackageModel) {
        this.OnDeleted.emit(packageModel);
    }
}