    
    
    // Global variables
    const file_uri = '/api/file';
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
                data.files.forEach((file) => {
                createTableRow(filesTable, [file]);
                });
        
                const dessertVotesTable = document.getElementById('dessertVotesTable').getElementsByTagName('tbody')[0];
                dessertVotesTable.innerHTML = '';
                Object.entries(data.dessertVotes).forEach(([dessert, votes]) => {
                    createTableRow(dessertVotesTable, [dessert, votes]);
                });
        
                const authorsList = document.getElementById('authorsList');
                if (!data.authorsList || data.authorsList.length === 0) {
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
                    for (let author of data.authorsList) {
                        const date = new Date(author.lastSubmissionTimestamp);
                        createTableRow(tbody, [author.name, author.countryOfOrigin, date.toLocaleString()]);
                    }
                    authorsList.appendChild(tbody);
                }

                // Weather
                let weather = data.currentWeather;
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
            return await response.json();
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
        event.preventDefault();
        const form = document.querySelector('#uploadForm');
        const formData = new FormData(form);

        try {
            let data;
            if (ajaxEnabled) {
                data = await ajaxRequest(file_uri, { method: 'POST', body: formData });
                await getFormData();
            } else {
                data = await fetchRequest(file_uri, { method: 'POST', body: formData });
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
                data = await ajaxRequest(file_uri);
            } else {
                data = await fetchRequest(file_uri);
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

    // Main //
    document.addEventListener('DOMContentLoaded', setupEventListeners);
    getFormData();