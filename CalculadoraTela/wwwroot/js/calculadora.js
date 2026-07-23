document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("calcForm");
    const inputs = document.querySelectorAll(".calc-input");
    const btnGuardar = document.getElementById("btnGuardar");

    // Función para enviar los datos por AJAX y actualizar la interfaz con los resultados
    function realizarCalculo() {
        // Recolectar los datos del formulario de manera estructurada para el ViewModel
        const formData = {
            TipoProducto: document.getElementById("TipoProducto") ? document.getElementById("TipoProducto").value : "Plana",
            UrdimbreTejido: parseFloat(document.querySelector('[name="UrdimbreTejido"]').value) || 0,
            CintaUrdimbre: parseFloat(document.querySelector('[name="CintaUrdimbre"]').value) || 0,
            UrdimbreDenier: parseFloat(document.querySelector('[name="UrdimbreDenier"]').value) || 0,
            TramaTejido: parseFloat(document.querySelector('[name="TramaTejido"]').value) || 0,
            CintaTrama: parseFloat(document.querySelector('[name="CintaTrama"]').value) || 0,
            TramaDenier: parseFloat(document.querySelector('[name="TramaDenier"]').value) || 0,
            UrdimbreRefuerzoTejido: parseFloat(document.querySelector('[name="UrdimbreRefuerzoTejido"]').value) || 0,
            CintaRefuerzo: parseFloat(document.querySelector('[name="CintaRefuerzo"]').value) || 0,
            DenierRefuerzo: parseFloat(document.querySelector('[name="DenierRefuerzo"]').value) || 0,
            AnchoRefuerzoFactor: parseFloat(document.querySelector('[name="AnchoRefuerzoFactor"]').value) || 0,
            Laminado: parseFloat(document.querySelector('[name="Laminado"]').value) || 0,
            Ancho: parseFloat(document.querySelector('[name="Ancho"]').value) || 0
        };

        fetch('/Home/CalcularAjax', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify(formData)
        })
        .then(response => response.json())
        .then(result => {
            if (result.success) {
                const data = result.data;

                // Actualizar Matriz Superior
                document.getElementById("resUrdimbreRes").innerText = data.resistenciaUrdimbre.toFixed(2) + " KgF";
                document.getElementById("resUrdimbrePeso").innerText = data.pesoUrdimbre.toFixed(2) + " gr";
                document.getElementById("resUrdimbrePorc").innerText = data.porcentajeUrdimbre.toFixed(1) + " %";

                document.getElementById("resTramaRes").innerText = data.resistenciaTrama.toFixed(2) + " KgF";
                document.getElementById("resTramaPeso").innerText = data.pesoTrama.toFixed(2) + " gr";
                document.getElementById("resTramaPorc").innerText = data.porcentajeTrama.toFixed(1) + " %";

                document.getElementById("resUrdRefRes").innerText = data.urdimbreRefuerzoResistencia.toFixed(2) + " KgF";
                document.getElementById("resCantidadConos").innerText = data.maquinaNumero;
                document.getElementById("resPesoMetro").innerText = data.pesoMetroLineal.toFixed(1) + " gml";

                // Actualizar Bloque de Resultados Inferior
                document.getElementById("lblTipoProductoRes").innerText = data.tipoProducto;
                document.getElementById("resMedida").innerText = `${data.ancho} x ${data.corte}`;
                document.getElementById("resTejidoConcatenado").innerText = `${data.urdimbreTejido} x ${data.tramaTejido}`;
                document.getElementById("resDenierConcatenado").innerText = `${data.urdimbreDenier} x ${data.tramaDenier}`;

                document.getElementById("resUrdimbreRes2").innerText = data.resistenciaUrdimbre.toFixed(2);
                document.getElementById("resUrdimbrePorc2").innerText = data.porcentajeUrdimbre.toFixed(0);
                document.getElementById("resTramaRes2").innerText = data.resistenciaTrama.toFixed(2);
                document.getElementById("resTramaPorc2").innerText = data.porcentajeTrama.toFixed(0);
                document.getElementById("resUrdRefRes2").innerText = data.urdimbreRefuerzoResistencia.toFixed(2);
                document.getElementById("resAnchoRefuerzoLabel").innerText = data.anchoRefuerzoFactor;

                document.getElementById("resPesoBase").innerText = data.pesoTejidoBase.toFixed(1);
                document.getElementById("resPesoLaminado").innerText = data.pesoConLaminado.toFixed(1);
                document.getElementById("resPesoRefuerzo").innerText = data.pesoConRefuerzo.toFixed(1);
                document.getElementById("resPesoMetro2").innerText = data.pesoMetroLineal.toFixed(1);
            }
        })
        .catch(error => console.error("Error en cálculo AJAX:", error));
    }

    // Escuchar cambios en cualquier input de la calculadora para recálculo automático en vivo
    inputs.forEach(input => {
        input.addEventListener("input", realizarCalculo);
        input.addEventListener("change", realizarCalculo);
    });

    // Evento del botón Guardar en Historial
    if (btnGuardar) {
        btnGuardar.addEventListener("click", function () {
            const formData = {
                TipoProducto: document.getElementById("TipoProducto") ? document.getElementById("TipoProducto").value : "Plana",
                UrdimbreTejido: parseFloat(document.querySelector('[name="UrdimbreTejido"]').value) || 0,
                CintaUrdimbre: parseFloat(document.querySelector('[name="CintaUrdimbre"]').value) || 0,
                UrdimbreDenier: parseFloat(document.querySelector('[name="UrdimbreDenier"]').value) || 0,
                TramaTejido: parseFloat(document.querySelector('[name="TramaTejido"]').value) || 0,
                CintaTrama: parseFloat(document.querySelector('[name="CintaTrama"]').value) || 0,
                TramaDenier: parseFloat(document.querySelector('[name="TramaDenier"]').value) || 0,
                UrdimbreRefuerzoTejido: parseFloat(document.querySelector('[name="UrdimbreRefuerzoTejido"]').value) || 0,
                CintaRefuerzo: parseFloat(document.querySelector('[name="CintaRefuerzo"]').value) || 0,
                DenierRefuerzo: parseFloat(document.querySelector('[name="DenierRefuerzo"]').value) || 0,
                AnchoRefuerzoFactor: parseFloat(document.querySelector('[name="AnchoRefuerzoFactor"]').value) || 0,
                Laminado: parseFloat(document.querySelector('[name="Laminado"]').value) || 0,
                Ancho: parseFloat(document.querySelector('[name="Ancho"]').value) || 0
            };

            fetch('/Home/GuardarHistorial', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(formData)
            })
            .then(response => response.json())
            .then(result => {
                if (result.success) {
                    alert("¡Registro guardado exitosamente en el historial!");
                    window.location.href = '/Home/Historial';
                } else {
                    alert("Error al guardar: " + result.message);
                }
            })
            .catch(error => {
                console.error("Error:", error);
                alert("Ocurrió un error al intentar guardar el registro.");
            });
        });
    }
});
