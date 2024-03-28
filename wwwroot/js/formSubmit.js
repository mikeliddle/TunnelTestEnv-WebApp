    
    
    // Global variables
    const form_uri = '/api/formupload';
    // Helper function to create a table row
    const createTableRow = (table, values) => {
        const row = table.insertRow();
        values.forEach((value) => {
        const cell = row.insertCell();
        cell.textContent = value;
        });
    };

    // Update the DOM with the fetched data
    function updateDOM(data) {
        if (data) {
            const filesTable = document.getElementById('filesTable').getElementsByTagName('tbody')[0];
            filesTable.innerHTML = '';
            if (!data.files || data.files.length === 0) {
                console.log('files is empty');
            } else {
                data.files.forEach(file => {
                    createTableRow(filesTable, [file]);
                });
            }
            const dessertVotesTable = document.getElementById('dessertVotesTable').getElementsByTagName('tbody')[0];
            dessertVotesTable.innerHTML = '';
            let storedFormData = data.storedFormData[0];
            if (!storedFormData.dessertVotes || storedFormData.dessertVotes.length === 0) {
                console.log('dessertVotes is empty');
            } else {
                storedFormData.dessertVotes.forEach(vote => {
                    createTableRow(dessertVotesTable, [vote.dessert, vote.votes]);
                });
            }
            const authorsList = document.getElementById('authorsList');
            if (!storedFormData.authorsList || storedFormData.authorsList.length === 0) {
                console.log('authorsList is empty');
            } 
            else 
            {
                authorsList.innerHTML = '';
        
                const thead = document.createElement('thead');
                const headerRow = thead.insertRow();
                const headers = ['Author', 'Country of Origin', 'Latest Update'];
                for (let header of headers) {
                    const th = document.createElement('th');
                    th.innerText = header;
                    headerRow.appendChild(th);
                }
                authorsList.appendChild(thead);
        
                const tbody = document.createElement('tbody');
                for (let author of storedFormData.authorsList) {
                    const date = new Date(author.lastSubmissionTimestamp);
                    createTableRow(tbody, [author.name, author.countryOfOrigin, date.toLocaleString()]);
                }
                authorsList.appendChild(tbody);
            }

            // Weather
            let weather = storedFormData.currentWeather;
            let iconSrc;
            switch (weather) {
                case 'rain':
                    iconSrc = '../rain.jpeg';
                    break;
                case 'sun':
                    iconSrc = '../sun.jpeg';
                    break;
                case 'snow':
                    iconSrc = '../snow.jpeg';
                    break;
                default:
                    weather = 'clear';
                    iconSrc = '../clear.jpeg';
                    break;
            }
            document.getElementById('weather-icon').src = iconSrc;
            document.getElementById('weather-text').innerText = weather;
        }
            else 
            {
            console.log('No data');
            }
    };
    
    async function fetchRequest(url, options = {}) {
        try {
            const response = await fetch(url, options);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            // Check if the response has any content
            const contentType = response.headers.get("content-type");
            if (contentType && contentType.includes("application/json")) {
                return await response.json();
            } else {
                // If there's no content, return a default object or value
                return {};
            }
        } catch (error) {
            console.error('There was a problem with the fetch operation: ', error);
        }
    }
    
    function ajaxRequest(uri, options = {}) {
        return new Promise((resolve, reject) => {
            var xhr = new XMLHttpRequest();
            xhr.open(options.method || 'GET', uri);
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4) {
                    if (xhr.status == 200) {
                        xhr.responseText ? resolve(JSON.parse(xhr.responseText)) : resolve();
                    } else {
                        reject('Error: ' + xhr.status);
                    }
                }
            };
            xhr.onerror = function () {
                reject('Network error');
            };
            if (options.method === 'POST') {
                xhr.send(options.body);
            } else {
                xhr.send();
            }
        }).catch(error => {
            console.error('There was a problem with the ajax request: ', error);
        });
    }

    const submitForm = async (event) => {
        if (!event){
            console.error('No event');
            return;
        }
        event.preventDefault();
        const form = document.querySelector('#uploadForm');
        const formData = new FormData(form);

        try {
            let data;
            if (ajaxEnabled) {
                data = await ajaxRequest(form_uri, { method: 'POST', body: formData });
                await getFormData();
            } else {
                data = await fetchRequest(form_uri, { method: 'POST', body: formData });
            }
        } catch (error) {
            console.error('Error:', error);
            // Display an error message to the user
        } finally {
            await getFormData();
            form.reset();
        }
    };

    // Fetch data and update the DOM
    const getFormData = async () => {
        try {
            let data;
            if (ajaxEnabled) {
                data = await ajaxRequest(form_uri);
            } else {
                data = await fetchRequest(form_uri);
            }
            updateDOM(data);
        } catch (error) {
            console.error('Error:', error);
        }
    }
    
    // Setup event listeners
    const setupEventListeners = () => {
        const form = document.querySelector('#uploadForm');
        if (form) {
            form.addEventListener('submit', submitForm);
        } else {
            console.error('Form not found');
        }
    };

    const clearData = async () => {
        // Clear data from the DOM
        const filesTable = document.getElementById('filesTable').getElementsByTagName('tbody')[0];
        filesTable.innerHTML = '';
    
        const dessertVotesTable = document.getElementById('dessertVotesTable').getElementsByTagName('tbody')[0];
        dessertVotesTable.innerHTML = '';
    
        const authorsList = document.getElementById('authorsList');
        authorsList.innerHTML = '';
    
        document.getElementById('weather-icon').src = '';
        document.getElementById('weather-text').innerText = '';
    
        await fetchRequest(form_uri, { method: 'DELETE' });
    
        // Fetch the updated data from the server
        await getFormData();
    };

    // Main //
    document.addEventListener('DOMContentLoaded', setupEventListeners);
    document.getElementById('clearDataButton').addEventListener('click', function() {
        clearData();
    });
    getFormData();