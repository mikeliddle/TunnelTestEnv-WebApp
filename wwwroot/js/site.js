const uri = 'api/todoitems';
let todos = [];
let ajaxEnabled = false;
let dataTable = null;

window.addEventListener('DOMContentLoaded', event => {

  // Toggle the side navigation
  const sidebarToggle = document.body.querySelector('#sidebarToggle');
  if (sidebarToggle) {
      if (localStorage.getItem('sb|sidebar-toggle') === 'true') {
          document.body.classList.toggle('sb-sidenav-toggled');
      }
      sidebarToggle.addEventListener('click', event => {
          event.preventDefault();
          document.body.classList.toggle('sb-sidenav-toggled');
          localStorage.setItem('sb|sidebar-toggle', document.body.classList.contains('sb-sidenav-toggled'));
      });
  }

  const ajaxToggle = document.getElementById('ajaxToggle');
  if (ajaxToggle) {
    ajaxToggle.addEventListener('change', event => {
      toggleAJAX();
    });
  }

  const dataTableHtml = document.getElementById('datatablesSimple');
  if (dataTableHtml) {
    dataTable = new simpleDatatables.DataTable(dataTableHtml);
  }
});

document.addEventListener('DOMContentLoaded', function () {
  var checkbox = document.querySelector('input[type="checkbox"]');

  checkbox.addEventListener('change', function () {
    toggleAJAX();
  });
});

function getIPAddress() {
  fetch("api/IPAddress")
    .then(response => response.json())
    .then(data => {
      let ipString = "Accessing site from: ";
      if (data["ipAddress"] == "##PROXY_IP##") {
        document.getElementById("ip_card").classList.add("bg-success");
        document.getElementById("ip_address_span").innerHTML=ipString + data["ipAddress"] + " (Proxy)";
      } else {
        document.getElementById("ip_card").classList.add("bg-warning");
        document.getElementById("ip_address_span").innerHTML=ipString + data["ipAddress"] + " (Not Proxy)";
      }        
    })
    .catch(error => console.error("unable to get ip address.", error));
}

function getItems() {
  if (ajaxEnabled) {
    const xhr = new XMLHttpRequest();
    xhr.onload = function () {
      if (xhr.status >= 200 && xhr.status < 300) {
        _displayItems(JSON.parse(xhr.response));
      } else {
        console.error('Unable to get items.');
      }
    }
    xhr.open('GET', uri);
    xhr.send();
  } else {
    fetch(uri)
      .then(response => response.json())
      .then(data => _displayItems(data))
      .catch(error => console.error('Unable to get items.', error));
  }
}

function addItem() {
  const addNameTextbox = document.getElementById('add-name');

  const item = {
    isComplete: false,
    name: addNameTextbox.value.trim()
  };

  if (ajaxEnabled) {
    const xhr = new XMLHttpRequest();
    xhr.onload = function (event) {
      if (xhr.status >= 200 && xhr.status < 300) {
        getItems();
        _displayItem(JSON.parse(event.target.response));
        addNameTextbox.value = '';
      } else {
        console.error('Unable to add item.');
      }
    }
    xhr.open('POST', uri);
    xhr.setRequestHeader('Content-Type', 'application/json');
    xhr.send(JSON.stringify(item));
  } else {
    fetch(uri, {
      method: 'POST',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(item)
    })
      .then(response => response.json())
      .then((data) => {
        _displayItem(data);
        getItems();
        addNameTextbox.value = '';
      })
      .catch(error => console.error('Unable to add item.', error));
  }
}

function deleteItem(id) {
  if (ajaxEnabled) {
    const xhr = new XMLHttpRequest();
    xhr.onload = function () {
      if (xhr.status >= 200 && xhr.status < 300) {
        let rows = dataTable.dom.querySelectorAll("td");
        rows.forEach(row => {
          if(row.innerText == id) {
            let index = row.parentNode.getAttribute("data-index");
            dataTable.rows.remove(parseInt(index));
          }
        });
        getItems();
      } else {
        console.error('Unable to delete item.');
      }
    }
    xhr.open('DELETE', `${uri}/${id}`);
    xhr.send();
  } else {
    fetch(`${uri}/${id}`, {
      method: 'DELETE'
    })
    .then(() => {
      let rows = dataTable.dom.querySelectorAll("td");
      rows.forEach(row => {
        if(row.innerText == id) {
          let index = row.parentNode.getAttribute("data-index");
          dataTable.rows.remove(parseInt(index));
        }
      });
      getItems();
    })
    .catch(error => console.error('Unable to delete item.', error));
  }
}

function updateItem(itemId) {
  const item = todos.find(item => item.id === itemId);
  item.isComplete = !item.isComplete;

  if (ajaxEnabled) {
    const xhr = new XMLHttpRequest();
    xhr.onload = function () {
      if (xhr.status >= 200 && xhr.status < 300) {
        getItems();
      } else {
        console.error('Unable to update item.');
      }
    }
    xhr.open('PUT', `${uri}/${itemId}`);
    xhr.setRequestHeader('Content-Type', 'application/json');
    xhr.send(JSON.stringify(item));
  } else {
    fetch(`${uri}/${itemId}`, {
      method: 'PUT',
      headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(item)
    })
    .then(() => getItems())
    .catch(error => console.error('Unable to update item.', error));
  }

  return false;
}

function toggleAJAX() {
  if (ajaxEnabled) {
    ajaxEnabled = false;
  } else {
    ajaxEnabled = true;
  }
}

function _displayItem(item) {
  let deleteButton = document.createElement('input');
  deleteButton.value = 'Delete';
  deleteButton.type = 'button';
  deleteButton.setAttribute('onclick', `deleteItem(${item.id})`);
  deleteButton = deleteButton.outerHTML;

  let isCompleteCheckbox = document.createElement('input');
  isCompleteCheckbox.type = 'checkbox';
  if(item.isComplete) {
    isCompleteCheckbox.setAttribute("checked", "");
  }
  isCompleteCheckbox.setAttribute('onchange', `updateItem(${item.id})`);
  isCompleteCheckbox = isCompleteCheckbox.outerHTML;

  dataTable.rows.add([item.id, item.name, isCompleteCheckbox, deleteButton]);
}

function _displayItems(data) {
  if (!dataTable.hasRows) {
    data.forEach(item => {
      _displayItem(item);
    });
  }
  todos = data;
}
