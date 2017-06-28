import { Directive, Input, Output, EventEmitter } from '@angular/core';

@Directive({
    selector: '[fileModel]',
    host: {
        "(change)": 'onChange($event)'
    }
})
export class FileModelDirective {
    private _fileReader: FileReader;
    private _file: any;

    @Input('fileModel') file: any;
    @Output() fileModelChange: EventEmitter<any> = new EventEmitter()

    constructor() {
        this._fileReader = new FileReader();
        this._fileReader.onload = (file) => {
            let binaryString = this._fileReader.result;
            let base64Encoded = btoa(binaryString);
            this.file = { FileName: this._file.name, Type: this._file.type, Size: this._file.size, Data: base64Encoded };
            this.fileModelChange.emit(this.file);
        }
    }

    onChange($event) {
        this._file = $event.target.files[0];
        this.encodeFile(this._file);
    }

    private encodeFile(file: File) {
        this._fileReader.readAsBinaryString(file);
    }
}