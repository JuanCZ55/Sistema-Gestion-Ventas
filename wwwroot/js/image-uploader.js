// image-uploader.js - Componente reutilizable para input de imagen con preview

// Función para inicializar el uploader en un modal específico
function initImageUploader(modalPrefix) {
  const input = document.getElementById(`${modalPrefix}_imageInput`);
  const preview = document.getElementById(`${modalPrefix}_preview`);
  const borrarHidden = document.getElementById(`${modalPrefix}_BorrarImagen`);

  if (!input || !preview || !borrarHidden) return; // Evitar errores si no existen

  // Función para limpiar la vista previa y resetear el input
  function clearPreview() {
    preview.innerHTML = "";
    try {
      input.value = "";
    } catch (e) {}
  }

  // Función para mostrar la vista previa de la imagen con contenedor y botón de cerrar
  function showImagePreview(src, filename) {
    preview.innerHTML = "";
    // Crear contenedor con clases CSS
    const container = document.createElement("div");
    container.className = "image-preview-container";

    // Crear imagen
    const img = document.createElement("img");
    img.src = src;
    img.alt = filename || "preview";
    img.className = "image-preview-img";

    // Crear botón de cerrar
    const closeBtn = document.createElement("button");
    closeBtn.type = "button";
    closeBtn.className = "btn-close image-preview-close";
    closeBtn.title = "Eliminar imagen";
    closeBtn.addEventListener("click", () => {
      clearPreview();
      borrarHidden.value = "true"; // Marcar para borrar
      try {
        URL.revokeObjectURL(src);
      } catch (e) {}
    });

    // Agregar elementos al contenedor
    container.appendChild(img);
    container.appendChild(closeBtn);
    preview.appendChild(container);
  }

  // Event listener para cambio en el input de archivo
  input.addEventListener("change", () => {
    const file = input.files[0];
    if (!file) return;
    // Validación simple: tipo y tamaño
    if (!file.type.startsWith("image/")) {
      showToast("danger", "Solo imágenes permitidas.");
      input.value = "";
      return;
    }
    if (file.size > 49 * 1024 * 1024) {
      showToast("danger", "Máximo 49MB.");
      input.value = "";
      return;
    }
    const url = URL.createObjectURL(file);
    showImagePreview(url, file.name);
    // Al seleccionar nueva imagen, asegurar BorrarImagen = false
    borrarHidden.value = "false";
  });

  // Exponer clearPreview si es necesario
  window[`clearPreview_${modalPrefix}`] = clearPreview;
}

// Función para setear imagen existente en edit modal
function setEditImage(url, modalPrefix = "edit") {
  const preview = document.getElementById(`${modalPrefix}_preview`);
  const borrarHidden = document.getElementById(`${modalPrefix}_BorrarImagen`);
  if (url && preview) {
    // Usar la función showImagePreview del uploader
    const container = document.createElement("div");
    container.className = "image-preview-container";

    const img = document.createElement("img");
    img.src = url;
    img.alt = "imagen_existente";
    img.className = "image-preview-img";

    const closeBtn = document.createElement("button");
    closeBtn.type = "button";
    closeBtn.className = "btn-close image-preview-close";
    closeBtn.title = "Eliminar imagen";
    closeBtn.addEventListener("click", () => {
      preview.innerHTML = "";
      borrarHidden.value = "true";
    });

    container.appendChild(img);
    container.appendChild(closeBtn);
    preview.appendChild(container);
    borrarHidden.value = "false";
  }
}
