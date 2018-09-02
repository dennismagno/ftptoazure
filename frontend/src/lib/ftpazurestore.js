import FTPAzureDispatcher from './ftpazuredispatcher.js';
import { EventEmitter } from 'events';

let _ftpfiles = [];
let _azurefiles = [];
let _successMessage = '';
let _errorMessage = '';

class FTPAzureStore extends EventEmitter {

    constructor() {
        super();
        this.dispatchToken = FTPAzureDispatcher.register(this.dispatcherCallback.bind(this))
    }

    loadFTPCallback(data) 
    {
        _ftpfiles = data;
    }

    loadAzureCallback(data) 
    {
        _azurefiles = data;
    }

    emitChange(eventName) {
        this.emit(eventName);
    }

    getFTPFiles() {
        return _ftpfiles;
    }

    getAzureFiles() {
        return _azurefiles;
    }

    getAPISuccessMessage() {
        return _successMessage;
    }

    getAPIErrorMessage() {
        return _errorMessage;
    }

    setAPISuccessMessage (message) {
        _successMessage = message;
    }

    setAPIErrorMessage (message) {
        _errorMessage = message;
    }

    addChangeListener(eventName, callback) {
        this.on(eventName, callback);
    }

    dispatcherCallback(action) {
        switch (action.actionType) {
            case 'FTPFILES_LOADED':
                this.loadFTPCallback(action.value);
                break;
            case 'AZUREFILES_LOADED':
                this.loadAzureCallback(action.value);
                break;
            case 'API_SUCCESS':
                this.setAPISuccessMessage(action.value);
                break;
            case 'API_ERROR':
                this.setAPIErrorMessage(action.value);
                break;
            default:
                break;
        }

        this.emitChange('STORE_' + action.actionType);

        return true;
    }
}

export default new FTPAzureStore();