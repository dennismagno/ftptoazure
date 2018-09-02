import React, { Component } from 'react';
import { Button, Icon, Table} from 'semantic-ui-react';
import './App.css';

class FileList extends Component {
    constructor(props) {
        super(props);        
		this.handleCopyButtonClick = this.handleCopyButtonClick.bind(this);
		this.handleDeleteButtonClick = this.handleDeleteButtonClick.bind(this);
    }
    
    handleCopyButtonClick(filename) {
		if (this.props.copyCallBack != null) this.props.copyCallBack(filename);
	}

	handleDeleteButtonClick(filename) {
        if (this.props.deleteCallBack != null) this.props.deleteCallBack(filename,this.props.deleteController);
	}

    triggerInputFile = () => this.fileInput.click()

    fileChangedHandler = (event) => {
        if (this.props.uploadCallBack != null) this.props.uploadCallBack(event.target.files[0],this.props.uploadController);
    }

    refreshClick = () => {
        if (this.props.refreshCallback != null) this.props.refreshCallback();
    }

    render() {
        var fileArray = this.props.files;
        return (
            <Table celled striped size='small'>
                <Table.Header>
                    <Table.Row>                    
                        <Table.HeaderCell colSpan='2'>
                            <input ref={fileInput => this.fileInput = fileInput} type="file" className='inputfile' accept="image/x-png,image/gif,image/jpeg" onChange={this.fileChangedHandler}/>
                            <Button floated='right' icon labelPosition='left' size='small' onClick={this.refreshClick}>
                                <Icon name='refresh' /> Reload
                            </Button>
                            <Button floated='right' icon labelPosition='left' primary size='small' onClick={this.triggerInputFile}>
                                <Icon name='upload' /> Upload
                            </Button>
                        </Table.HeaderCell>
                    </Table.Row>
                    <Table.Row>
                        <Table.HeaderCell colSpan='2'>{this.props.title}</Table.HeaderCell>
                    </Table.Row>
                    <Table.Row>
                        <Table.HeaderCell>Filename</Table.HeaderCell>
                        <Table.HeaderCell>Action</Table.HeaderCell>
                    </Table.Row>
                </Table.Header>
                <Table.Body>
                    {fileArray.map((item,i) =>                        
                        <Table.Row key={i}>
                            <Table.Cell><Icon name='file outline' /> {item.Name}</Table.Cell>
                            <Table.Cell textAlign='right' width={2}>
                                <Button.Group>
                                    <Button icon color='yellow' disabled={this.props.disableCopy} onClick={() => {this.handleCopyButtonClick(item.Name)} }>
                                        <Icon name='copy' />
                                    </Button>
                                    <Button icon color='red' onClick={() => {this.handleDeleteButtonClick(item.Name)} }>
                                        <Icon name='trash' />
                                    </Button>
                                </Button.Group>
                            </Table.Cell>
                        </Table.Row>
                    )}
                </Table.Body>
            </Table>
        );
    }
}

export default FileList;
