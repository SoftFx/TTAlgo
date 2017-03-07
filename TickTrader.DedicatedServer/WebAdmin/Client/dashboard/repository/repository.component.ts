import { Component, OnInit, OnDestroy } from '@angular/core';
import { ApiService, FeedService, ToastrService } from '../../services/index';
import { PackageModel, PluginModel, ResponseStatus } from '../../models/index';
import { ViewChild } from '@angular/core';

@Component({
    selector: 'repository-cmp',
    template: require('./repository.component.html'),
    styles: [require('../../app.component.css')],
})

export class RepositoryComponent implements OnInit {
    private _isFileDuplicated: boolean = false;
    private _uploadingError: ResponseStatus;

    public SelectedFile: any;
    public Packages: PackageModel[] = [];
    public Uploading: boolean = false;


    @ViewChild('PackageInput')
    PackageInput: any;

    constructor(private Api: ApiService, private Toastr: ToastrService) {
    }

    ngOnInit() {
        this.Api.Feed.addPackage.subscribe(algoPackage => this.addPackage(algoPackage));
        this.Api.Feed.deletePackage.subscribe(pname => this.deletePackage(pname));

        this.refreshPackages();
    }

    public get SelectedFileName() {
        if (this.SelectedFile)
            return this.SelectedFile.name;
        else
            return "";
    }

    public DeletePackage(algoPackage: PackageModel) {
        this.Api
            .deleteAlgoPackage(algoPackage.Name)
            .subscribe(() => this.deletePackage(algoPackage.Name),
            err => this.Toastr.error("Failed to execute the query. Please try again."));
    }

    public UploadPackage() {
        this._uploadingError = null;
        this.Uploading = true;

        this.Api
            .uploadAlgoPackage(this.SelectedFile)
            .finally(() => { this.Uploading = false; })
            .subscribe(res => {
                this.SelectedFile = null;
                this.PackageInput.nativeElement.value = "";
                this.refreshPackages();
            },
            err => {
                try {
                    this._uploadingError = err.json() as ResponseStatus;
                }
                catch (err) {
                    this.Toastr.error("Failed to execute the query. Please try again.")
                }
            });
    }

    public OnFileInputChange(event) {
        this._uploadingError = null;
        this.SelectedFile = event.target.files[0];
    }

    public get FileInputError() {
        if ((this._uploadingError != null && this._uploadingError['code'] && this._uploadingError.code == 101) || this.isFileDuplicated) {
            return 'DuplicatePackage';
        }
        return null;
    }

    public get IsFileNameVaild(): boolean {
        let a = this.SelectedFile && !this.SelectedFile.name && !this.isFileDuplicated;
        return a;
    }

    public get CanUpload() {
        return !this.Uploading
            && this.SelectedFileName
            && !this.isFileDuplicated
            && (!this._uploadingError || !this._uploadingError['code'] || this._uploadingError['code'] != 101);
    }

    private get isFileDuplicated(): boolean {
        return this.Packages.find(p => this.SelectedFile && p.Name == this.SelectedFile.name) != null;
    }

    private refreshPackages() {
        this.Api.getAlgoPackages()
            .subscribe(res => this.Packages = res);
    }

    private addPackage(packageModel: PackageModel) {
        if (!this.Packages.find(p => p.Name === packageModel.Name))
            this.Packages.push(packageModel);
    }

    private deletePackage(packageName: string) {
        this.Packages = this.Packages.filter(p => p.Name !== packageName);
    }

}
