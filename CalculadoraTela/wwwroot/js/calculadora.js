document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("calcForm");
    if (!form) return;

    const inputs = form.querySelectorAll(".calc-input, #TipoProducto");

    inputs.forEach(input => {
        input.addEventListener("input", calcularEnTiempoReal);
        input.addEventListener("change", calcularEnTiempoReal);
    });

    function calcularEnTiempoReal() {
        const formData = new FormData(form);
        const data = {};
        formData.forEach((value, key) => {
            data[key] = value;
        });

        fetch('/Home/CalcularAjax', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
            },
            body: JSON.stringify(data)
        })
        .then(response => response.json())
        .then(res => {
            if (res.success) {
                const model = res.data;

                // Matriz superior
                document.getElementById("resUrdimbreRes").innerText = (model.resistenciaUrdimbre || 0).toFixed(2) + " KgF";
                document.getElementById("resUrdimbrePeso").innerText = (model.pesoUrdimbre || 0).toFixed(2) + " gr";
                document.getElementById("resUrdimbrePorc").innerText = (model.porcentajeUrdimbre || 0).toFixed(1) + " %";

                document.getElementById("resTramaRes").innerText = (model.resistenciaTrama || 0).toFixed(2) + " KgF";
                document.getElementById("resTramaPeso").innerText = (model.pesoTrama || 0).toFixed(2) + " gr";
                document.getElementById("resTramaPorc").innerText = (model.porcentajeTrama || 0).toFixed(1) + " %";

                document.getElementById("resUrdRefRes").innerText = (model.urdimbreRefuerzoResistencia || 0).toFixed(2) + " KgF";
                
                // Cantidad de Conos (REDONDEA.PAR)
                document.getElementById("resCantidadConos").innerText = model.maquinaNumero || 0;
                
                document.getElementById("resPesoMetro").innerText = (model.pesoMetroLineal || 0).toFixed(1) + " gml";

                // Bloque de Resultados Inferior
                document.getElementById("lblTipoProductoRes").innerText = model.tipoProducto || "";
                document.getElementById("resMedida").innerText = `${model.ancho || 0} x ${model.corte || 0}`;
                document.getElementById("resTejidoConcatenado").innerText = `${model.urdimbreTejido || 0}x${model.tramaTejido || 0}`;
                document.getElementById("resDenierConcatenado").innerText = `${model.urdimbreDenier || 0}x${model.tramaDenier || 0}`;

                document.getElementById("resUrdimbreRes2").innerText = (model.resistenciaUrdimbre || 0).toFixed(2);
                document.getElementById("resUrdimbrePorc2").innerText = (model.porcentajeUrdimbre || 0).toFixed(0);
                
                document.getElementById("resTramaRes2").innerText = (model.resistenciaTrama || 0).toFixed(2);
                document.getElementById("resTramaPorc2").innerText = (model.porcentajeTrama || 0).toFixed(0);

                document.getElementById("resUrdRefRes2").innerText = (model.urdimbreRefuerzoResistencia || 0).toFixed(2);
                document.getElementById("resAnchoRefuerzoLabel").innerText = model.anchoRefuerzoFactor || 0;
                
                document.getElementById("resPesoBase").innerText = (model.pesoTejidoBase || 0).toFixed(1);
                document.getElementById("resPesoLaminado").innerText = (model.pesoConLaminado || 0).toFixed(1);
                document.getElementById("resPesoRefuerzo").innerText = (model.pesoConRefuerzo || 0).toFixed(1); // GMP
                document.getElementById("resPesoMetro2").innerText = (model.pesoMetroLineal || 0).toFixed(1); // GML
            }
        })
        .catch(error => console.error("Error en el cálculo AJAX:", error));
    }

    // Botón Guardar en Historial por AJAX
    const btnGuardar = document.getElementById("btnGuardar");
    if (btnGuardar) {
        btnGuardar.addEventListener("click", function () {
            const formData = new FormData(form);
            const data = {};
            formData.forEach((value, key) => { data[key] = value; });

            fetch('/Home/GuardarHistorial', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify(data)
            })
            .then(response => response.json())
            .then(res => {
                if (res.success) {
                    alert("¡Registro guardado correctamente en la base de datos!");
                    window.location.href = '/Home/Historial';
                } else {
                    alert("Error al guardar: " + (res.message || "Desconocido"));
                }
            })
            .catch(error => console.error("Error al guardar:", error));
        });
    }
});
