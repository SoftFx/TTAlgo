import {Injectable, Inject} from '@angular/core';
import { Resource, GuiEn } from '../resx/index';


@Injectable()
export class ResourceService {
	private _currentLang: string;
    private readonly resources: any;

	public get currentLang() {
	  return this._currentLang;
	}

	constructor() {
        this._currentLang = GuiEn.name;
        this.resources = { [GuiEn.name]: GuiEn.dictionary }
	}

	public use(lang: string): void {
		this._currentLang = lang;
	}

	private pop(key: string): string {
        if (this.resources[this.currentLang] && this.resources[this.currentLang][key]) {
			return this.resources[this.currentLang][key];
		}

		return key;
	}

	public instant(key: string) {
		return this.pop(key); 
	}
}