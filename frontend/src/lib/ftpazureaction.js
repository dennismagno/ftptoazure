import FTPAzureDispatcher from './ftpazuredispatcher';

class FTPAzureActions {
    loadFTPFiles()
    {
        fetch('api/ftpfiles')
        .then(res => res.json())
        .then(data =>  FTPAzureDispatcher.dispatch({
            actionType: 'FTPFILES_LOADED',
            value: data
        }));
    }

    loadAzureFiles()
    {
        fetch('api/azurefiles')
        .then(res => res.json())
        .then(data => FTPAzureDispatcher.dispatch({
            actionType: 'AZUREFILES_LOADED',
            value: data
        }));
    }

    apiDeleteFile(filename,controller) {
        fetch('api/' + controller, {
            method: "POST", 
            headers: {
                "filename": filename,
            },
        })
        .then(function(response) {
            if (!response.ok) {
                throw Error(response.statusText);
            }
            return response.json();
        }).then(function(data) {
            FTPAzureDispatcher.dispatch({
                actionType: 'API_SUCCESS',
                value: data
            });
        }).catch(function(error) {
            console.log(error);
        });
    }

    sendToAzure(filename) {
        fetch('api/ftptoazure', {
            method: "POST", 
            headers: {
                "filename": filename,
            },
        })
        .then(function(response) {
            if (!response.ok) {
                throw Error(response.statusText);
            }
            return response.json();
        }).then(function(data) {
            FTPAzureDispatcher.dispatch({
                actionType: 'API_SUCCESS',
                value: data
            });
        }).catch(function(error) {
            console.log(error);
        });
    }

    apiUpload(file,controller) {
        console.log(file);
        let formData = new FormData();
        formData.append('file', file);

        const options = { 
            method: 'POST',
            body: formData, 
        };
        
        fetch('api/' + controller, options)
        .then(function(response) {
            if (!response.ok) {
                throw Error(response.statusText);
            }
            return response.json();
        }).then(function(data) {
            FTPAzureDispatcher.dispatch({
                actionType: 'API_SUCCESS',
                value: data
            });
        }).catch(function(error) {
            console.log(error);
        });
    }
}



export default new FTPAzureActions()