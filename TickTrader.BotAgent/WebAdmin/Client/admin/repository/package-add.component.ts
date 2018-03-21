import { Component, ChangeDetectionStrategy, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { ApiService, ToastrService } from '../../services/index';
import { ResponseStatus, ObservableRequest, ResponseCode } from '../../models/index';
import { Location } from '@angular/common';

@Component({
    selector: 'package-add-cmp',
    template: require('./package-add.component.html'),
    styles: [require('../../app.component.css')]
})

export class PackageAddComponent implements OnInit {
    public UploadRequest: ObservableRequest<void>;
    public CheckPackageRequest: ObservableRequest<void>;
    public SelectedFile: any;

    @ViewChild('PackageInput')
    PackageInput: any;

    constructor(private _api: ApiService, private _toastr: ToastrService, private _location: Location) { }

    ngOnInit() {
        this.UploadRequest = null;
        this.CheckPackageRequest = null;
    }

    public get SelectedFileName() {
        if (this.SelectedFile)
            return this.SelectedFile.name;
        else
            return "";
    }

    public InitUploadPackage() {
        this.CheckPackageRequest = new ObservableRequest(this._api.PackageExists(this.SelectedFileName))
            .Subscribe(ok => { },
            err => {
                if (err.Status !== 404) {
                    this.CheckPackageRequest = null;
                    this._toastr.error(err.Message);
                }
                else if (err.Status === 404)
                    this.UploadPackage();
            });
    }

    public UploadPackage() {
        this.CheckPackageRequest = null;

        this.UploadRequest = new ObservableRequest(this._api.UploadPackage(this.SelectedFile))
            .Subscribe(ok => {
                this.SelectedFile = null;
                this.PackageInput.nativeElement.value = "";
                this._location.back();
            },
            err => {
                if (!err.Handled) {
                    this._toastr.error(err.Message);
                }
            })
    }

    CancelUploadingPackage() {
        this.CheckPackageRequest = null;
    }

    public OnFileInputChange(event) {
        this.UploadRequest = null;
        this.CheckPackageRequest = null;
        this.SelectedFile = event.target.files[0];
    }

    public get CanUpload() {
        return !!this.SelectedFile && (!this.UploadRequest || this.UploadRequest && this.UploadRequest.IsCompleted);
    }

    public Cancel() {
        this.UploadRequest = null;
        this._location.back();
    }
}
