document.addEventListener("DOMContentLoaded", function () {
    const inputs = document.querySelectorAll(".calc-input");

    // Asignar eventos a todos los inputs y selects con la clase calc-input
    inputs.forEach(input => {
        input.addEventListener("input", actualizarVistaEnVivo);
        input.addEventListener("change", actualizarVistaEnVivo);
    });

    // Botón de guardar en historial
    const btnGuardar = document.getElementById("btnGuardar");
    if (btnGuardar) {
        btnGuardar.addEventListener("click", enviarFormulario);
    }

    function actualizarVistaEnVivo() {
        // Obtener valores de manera segura
        const tipoProducto = document.getElementById("TipoProducto")?.value || "Plana";
        const urdimbreTejido = document.getElementById("UrdimbreTejido")?.value || "0";
        const tramaTejido = document.getElementById("TramaTejido")?.value || "0";
        const urdimbreDenier = document.getElementById("UrdimbreDenier")?.value || "0";
        const tramaDenier = document.getElementById("TramaDenier")?.value || "0";
        const anchoRefuerzo = document.getElementById("AnchoRefuerzoFactor")?.value || "0";

        // Actualizar textos espejo en la sección de Resultados de forma instantánea
        const lblTipoProd = document.getElementById("lblTipoProductoRes");
        if (lblTipoProd) lblTipoProd.textContent = tipoProducto;

        const resTejido = document.getElementById("resTejidoConcatenado");
        if (resTejido) resTejido.textContent = `${urdimbreTejido} x ${tramaTejido}`;

        const resDenier = document.getElementById("resDenierConcatenado");
        if (resDenier) resDenier.textContent = `${urdimbreDenier} x ${tramaDenier}`;

        const resAnchoRef = document.getElementById("resAnchoRefuerzoLabel");
        if (resAnchoRef) resAnchoRef.textContent = anchoRefuerzo;
    }

    function enviarFormulario() {
        const formData = {
            TipoProducto: document.getElementById("TipoProducto")?.value,
            UrdimbreTejido: parseFloat(document.getElementById("UrdimbreTejido")?.value) || 0,
            CintaUrdimbre: parseFloat(document.getElementById("CintaUrdimbre")?.value) || 0,
            UrdimbreDenier: parseFloat(document.getElementById("UrdimbreDenier")?.value) || 0,
            TramaTejido: parseFloat(document.getElementById("TramaTejido")?.value) || 0,
            CintaTrama: parseFloat(document.getElementById("CintaTrama")?.value) || 0,
            TramaDenier: parseFloat(document.getElementById("TramaDenier")?.value) || 0,
            UrdimbreRefuerzoTejido: parseFloat(document.getElementById("UrdimbreRefuerzoTejido")?.value) || 0,
            CintaRefuerzo: parseFloat(document.getElementById("CintaRefuerzo")?.value) || 0,
            DenierRefuerzo: parseFloat(document.getElementById("DenierRefuerzo")?.value) || 0,
            AnchoRefuerzoFactor: parseFloat(document.getElementById("AnchoRefuerzoFactor")?.value) || 0,
            Laminado: parseFloat(document.getElementById("Laminado")?.value) || 0,
            Ancho: parseFloat(document.getElementById("Ancho")?.value) || 0
        };

        // Petición AJAX para guardar en la base de datos
        fetch('/Home/GuardarHistorial', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        })
        .then(response => response.json())
        .then(data => {
            if (data && data.success) {
                alert("¡Registro guardado con éxito en el historial!");
                window.location.href = '/Home/Historial';
            } else {
                alert("Error al guardar el registro: " + (data?.message || "Verifica los datos."));
            }
        })
        .catch(error => {
            console.error("Error en la solicitud:", error);
            alert("Ocurrió un error al intentar conectar con el servidor.");
        });
    }
});
