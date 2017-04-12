import { Input, EventEmitter, Output, Component } from '@angular/core';
import { PackageModel, PluginModel } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';

@Component({
    selector: 'package-card-cmp',
    template: require('./package-card.component.html'),
    styles: [require('../../app.component.css')],
})

export class PackageCardComponent {

    constructor(private _api: ApiService, private _toastr: ToastrService){ }
    
    @Input() Package: PackageModel;
    @Output() OnDeleted = new EventEmitter<PackageModel>();

    public Delete() {
        this._api
            .DeletePackage(this.Package.Name)
            .subscribe(() => this.OnDeleted.emit(this.Package),
            err => this._toastr.error(`Error deleting package ${this.Package.Name}`));
    }
}