import { Input, EventEmitter, Output, Component, OnInit } from '@angular/core';
import { PackageModel, ObservableRequest } from '../../models/index';
import { ApiService, ToastrService } from '../../services/index';

@Component({
    selector: 'package-delete-cmp',
    template: require('./package-delete.component.html'),
    styles: [require('../../app.component.css')],
})

export class PacakgeDeleteComponent implements OnInit {

    public DeleteRequest: ObservableRequest<void>;

    constructor(private _api: ApiService, private _toastr: ToastrService) { }

    @Input() Package: PackageModel;
    @Output() OnDeleted = new EventEmitter<PackageModel>();
    @Output() OnCanceled = new EventEmitter<void>();


    ngOnInit() {
        this.DeleteRequest = null;
    }

    public Delete() {
        this.DeleteRequest = new ObservableRequest(this._api.DeletePackage(this.Package.Id))
            .Subscribe(ok => this.OnDeleted.emit(this.Package),
            err => {
                if (!err.Handled) {
                    this._toastr.error(err.Message);
                    this.DeleteRequest = null;
                }
            })
    }

    public Cancel() {
        this.OnCanceled.emit();
    }
}
