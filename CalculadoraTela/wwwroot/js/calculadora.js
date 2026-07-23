document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("calcForm");
    const inputs = form.querySelectorAll(".calc-input, #TipoProducto");

    inputs.forEach(input => {
        input.addEventListener("input", calcularEnTiempoReal);
        input.addEventListener("change", calcularEnTiempoReal);
    });

    function calcularEnTiempoReal() {
        const formData = new FormData(form);
        
        // Convertir FormData a un objeto plano JSON para enviarlo por fetch
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

                // Actualizar valores en la Matriz
                document.getElementById("resUrdimbreRes").innerText = model.resistenciaUrdimbre.toFixed(2) + " KgF";
                document.getElementById("resUrdimbrePeso").innerText = model.pesoUrdimbre.toFixed(2) + " gr";
                document.getElementById("resUrdimbrePorc").innerText = model.porcentajeUrdimbre.toFixed(1) + " %";

                document.getElementById("resTramaRes").innerText = model.resistenciaTrama.toFixed(2) + " KgF";
                document.getElementById("resTramaPeso").innerText = model.pesoTrama.toFixed(2) + " gr";
                document.getElementById("resTramaPorc").innerText = model.porcentajeTrama.toFixed(1) + " %";

                document.getElementById("resUrdRefRes").innerText = model.urdimbreRefuerzoResistencia.toFixed(2) + " KgF";
                document.getElementById("resPesoMetro").innerText = model.pesoMetroLineal.toFixed(1) + " gml";

                // Actualizar Bloque de Resultados
                document.getElementById("lblTipoProductoRes").innerText = model.tipoProducto;
                document.getElementById("resMedida").innerText = `${model.ancho} x ${model.corte}`;
                document.getElementById("resUrdimbreRes2").innerText = model.resistenciaUrdimbre.toFixed(2);
                document.getElementById("resUrdimbrePorc2").innerText = model.porcentajeUrdimbre.toFixed(0);
                
                document.getElementById("resTramaRes2").innerText = model.resistenciaTrama.toFixed(2);
                document.getElementById("resTramaPorc2").innerText = model.porcentajeTrama.toFixed(0);

                document.getElementById("resUrdRefRes2").innerText = model.urdimbreRefuerzoResistencia.toFixed(2);
                
                document.getElementById("resPesoBase").innerText = model.pesoTejidoBase.toFixed(1);
                document.getElementById("resPesoLaminado").innerText = model.pesoConLaminado.toFixed(1);
                document.getElementById("resPesoRefuerzo").innerText = model.pesoConRefuerzo.toFixed(1);
                document.getElementById("resPesoMetro2").innerText = model.pesoMetroLineal.toFixed(1);
            }
        })
        .catch(error => console.error("Error en el cálculo AJAX:", error));
    }

    // Botón Guardar en Historial por AJAX (Sincronizado con el Controlador)
    const btnGuardar = document.getElementById("btnGuardar");
    if (btnGuardar) {
        btnGuardar.addEventListener("click", function () {
            const formData = new FormData(form);
            const data = {};
            formData.forEach((value, key) => {
                data[key] = value;
            });

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
            .catch(error => {
                console.error("Error al guardar en historial:", error);
                alert("Ocurrió un error al intentar guardar el registro.");
            });
        });
    }
});
