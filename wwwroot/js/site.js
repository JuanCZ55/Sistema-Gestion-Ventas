// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Inicialización de Tom Select
document.addEventListener("DOMContentLoaded", function () {
  new TomSelect("#user-select", {
    valueField: "id",
    labelField: "nombre",
    searchField: ["codigo", "nombre"],
    persist: false,
    cache: false,
    load: function (query, callback) {
      if (query.length < 1) return callback([]);
      fetch(`/api/productos/search?q=${encodeURIComponent(query)}`)
        .then((r) => r.json())
        .then((data) => callback(data))
        .catch(() => callback());
    },
    onType: function (str) {
      if (str === "") {
        this.clearOptions();
      }
    },
    onChange: function (id) {
      if (!id) return;
      onSeleccion(id);
    },
    render: {
      option: function (item, escape) {
        return `
          <div class="text-capitalize">[${escape(item.codigo)}] ${escape(item.nombre)} — $${escape(item.precioVenta)}</div>
        `;
      },
      item: function (item, escape) {
        return `<div>${escape(item.nombre)}</div>`;
      },
    },
  });
});

// Función para mostrar Toasts
window.showToast = function (type, message) {
  const toastEl = document.getElementById("appToast");
  const body = document.getElementById("appToastBody");
  toastEl.className = `toast text-white bg-${type} border-0`;
  body.innerHTML = message;
  const toast = new bootstrap.Toast(toastEl, { delay: 3000 });
  toast.show();
};

// Atajo de teclado para enfocar Tom Select con '|'
document.addEventListener("keydown", function (e) {
  if (e.key === "|") {
    e.preventDefault();
    const tsInput = document.querySelector("#user-select + .ts-wrapper input");
    if (tsInput) {
      tsInput.focus();
    }
  }
});
