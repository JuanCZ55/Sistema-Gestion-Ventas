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
    onType: function () {
      this.clearOptions();
      this.lastQuery = null;
    },
    onBlur: function () {
      this.clearOptions();
      this.lastQuery = null;
    },
    onChange: function (value) {
      if (!value) return;

      // Hacer fetch al API para obtener detalles del producto
      fetch(`/api/productos/${value}`)
        .then((response) => {
          if (!response.ok) {
            throw new Error("Error al cargar el producto");
          }
          return response.json();
        })
        .then((data) => {
          // Poblar los campos del modal con los datos del producto
          document.getElementById("detail-codigo").textContent = data.codigo;
          document.getElementById("detail-nombre").textContent = data.nombre;
          document.getElementById("detail-precio").textContent =
            `$${data.precioVenta}`;
          document.getElementById("detail-stock").textContent = data.stock;
          document.getElementById("detail-categoria").textContent =
            data.categoria?.nombre ?? "Sin categoría";
          document.getElementById("detail-proveedor").textContent =
            data.proveedor?.nombreContacto ?? "Sin proveedor";

          // Manejar la imagen
          const imgElement = document.getElementById("detail-imagen");
          const noImgElement = document.getElementById("detail-no-imagen");

          if (data.imagen) {
            imgElement.src = data.imagen;
            imgElement.style.display = "block";
            noImgElement.style.display = "none";
          } else {
            imgElement.style.display = "none";
            noImgElement.style.display = "block";
          }

          // Abrir el modal
          const modal = new bootstrap.Modal(
            document.getElementById("productDetailsModal"),
          );
          this.clear();
          this.clearOptions();
          this.lastQuery = null;
          modal.show();
        })
        .catch((error) => {
          console.error("Error:", error);
          showToast("danger", "Error al cargar los detalles del producto");
        });
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
