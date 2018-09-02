import React, { Component } from 'react';
import FileList from './FileList.js';
import { Modal, Button, Icon,  Header } from 'semantic-ui-react';
import './App.css';
import FTPAzureActions from './lib/ftpazureaction.js';
import FTPAzureStore from './lib/ftpazurestore.js'

class App extends Component {
  
  constructor(props) {
      super(props);
      this.state = { ftpfiles: [],azurefiles: [], open: false,modalTitle: '', modalMessage: '', modalYesNo: true};
      this.onLoadFTPFiles = this.onLoadFTPFiles.bind(this);
      this.onLoadAzureFiles = this.onLoadAzureFiles.bind(this);
      this.APISuccessMessage = this.APISuccessMessage.bind(this);
      this.handleModalClose = this.handleModalClose.bind(this);
      this.ftpCopyClick = this.ftpCopyClick.bind(this);
  }

  componentDidMount() {
    FTPAzureStore.addChangeListener('STORE_FTPFILES_LOADED', this.onLoadFTPFiles);
    FTPAzureStore.addChangeListener('STORE_AZUREFILES_LOADED', this.onLoadAzureFiles);
    FTPAzureStore.addChangeListener('STORE_API_SUCCESS', this.APISuccessMessage);
    FTPAzureActions.loadFTPFiles();
    FTPAzureActions.loadAzureFiles();
  }

  onLoadFTPFiles()
  {
    this.listFTPFiles()
  }

  onLoadAzureFiles()
  {
    this.listAzureFiles()
  }

  listFTPFiles() {
    this.setState({
      ftpfiles: FTPAzureStore.getFTPFiles()
    })
  }

  listAzureFiles() {
    this.setState({
      azurefiles: FTPAzureStore.getAzureFiles()
    })
  }
  
  ftpCopyClick(filename) {    
    FTPAzureActions.sendToAzure(filename);
  }

  uploadClick = (file,action) => {
    FTPAzureActions.apiUpload(file,action);
  }

  deleteClick = (filename,action) => {
    FTPAzureActions.apiDeleteFile(filename,action);
  }

  APISuccessMessage()
  {
    this.setState({
			open: true,
			modalTitle: 'Success',
			modalButton2: 'Ok',
			modalMessage: FTPAzureStore.getAPISuccessMessage(),
			modalYesNo: false
    });
    
    FTPAzureActions.loadAzureFiles();
    FTPAzureActions.loadFTPFiles();
  }

  handleModalClose() {
		this.setState({
			open: false
		});
	}

  render() {
    return (
      <div className="App">
        <Modal basic size='small' open={ this.state.open } onClose={ this.handleModalClose }>
          <Header icon={this.state.modalButton2 === 'Ok' ? 'info' : 'warning'} content={this.state.modalTitle} />
          <Modal.Content>
          <p>{ this.state.modalMessage }</p>
          </Modal.Content>
          <Modal.Actions>
            <Button color='green' inverted onClick={ this.handleModalClose }>
              <Icon name='checkmark' /> { this.state.modalButton2 }
            </Button>
          </Modal.Actions>
        </Modal>
        <div className="ui stackable inverted divided equal height stackable grid">
          <div className="eight wide column">
            <FileList title={'FTP Files'} 
              disableCopy={false} files={this.state.ftpfiles} 
              copyCallBack={this.ftpCopyClick} 
              uploadCallBack={this.uploadClick} 
              uploadController={'ftpupload'} 
              deleteCallBack={this.deleteClick} 
              deleteController={'ftpdelete'} 
              refreshCallback={() => {FTPAzureActions.loadFTPFiles()}}/>
          </div>
          <div className="eight wide column">
            <FileList title={'Azure BLOB Storage Files'} 
              disableCopy={true} files={this.state.azurefiles} 
              uploadCallBack={this.uploadClick} 
              uploadController={'azureupload'} 
              deleteCallBack={this.deleteClick} 
              deleteController={'azuredelete'} 
              refreshCallback={() => {FTPAzureActions.loadAzureFiles()}}/>
          </div>
        </div>
      </div>
    );
  }
}

export default App;
