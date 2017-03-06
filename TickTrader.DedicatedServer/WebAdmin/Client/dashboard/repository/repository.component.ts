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

    public selectedFile: any;
    public packages: PackageModel[] = [];
    public uploading: boolean = false;
    public uploadingError: ResponseStatus;

    @ViewChild('packageInput')
    packageInput: any;

    constructor(private api: ApiService, private toastr: ToastrService) {
    }

    ngOnInit() {
        this.api.feed.addPackage.subscribe(algoPackage => {
            this.packages.push(algoPackage);
        });
        this.api.feed.deletePackage.subscribe(pname => {
            this.packages = this.packages.filter(p => p.Name !== pname);
        });

        this.refreshPackages();
    }

    public get selectedFileName() {
        if (this.selectedFile)
            return this.selectedFile.name;
        else
            return "";
    }

    private refreshPackages() {
        this.api.getAlgoPackages()
            .subscribe(res => {
                this.packages = res;
            });
    }

    public deletePackage(algoPackage: PackageModel) {
        this.api
            .deleteAlgoPackage(algoPackage.Name)
            .subscribe(() => {
                this.packages = this.packages.filter(p => p !== algoPackage);
            },
            err => {
                this.toastr.error("Failed to execute the query. Please try again.");
            });
    }

    public uploadPackage() {
        this.uploadingError = null;
        this.uploading = true;

        this.api
            .uploadAlgoPackage(this.selectedFile)
            .finally(() => { this.uploading = false; })
            .subscribe(res => {
                this.selectedFile = null;
                this.packageInput.nativeElement.value = "";
                this.refreshPackages();
            },
            err => {
                try {
                    this.uploadingError = err.json() as ResponseStatus;
                }
                catch (err) {
                    this.toastr.error("Failed to execute the query. Please try again.")
                }
            });
    }

    public onFileInputChange(event) {
        this.uploadingError = null;
        this.selectedFile = event.target.files[0];
    }

    public get fileInputError() {
        if ((this.uploadingError != null && this.uploadingError['code'] && this.uploadingError.code == 101) || this.isFileDuplicated) {
            return 'DuplicatePackage';
        }
        return null;
    }

    public get isFileNameVaild(): boolean {
        let a = this.selectedFile && !this.selectedFile.name && !this.isFileDuplicated;
        return a;
    }

    private get isFileDuplicated(): boolean {
        return this.packages.find(p => this.selectedFile && p.Name == this.selectedFile.name) != null;
    }

    public get canUpload() {
        return !this.uploading
            && this.selectedFileName
            && !this.isFileDuplicated
            && (!this.uploadingError || !this.uploadingError['code'] || this.uploadingError['code'] != 101);
    }
}
