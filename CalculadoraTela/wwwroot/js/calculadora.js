document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("calcForm");
    if (!form) return;

    // Escuchar cambios en cualquier input, select o elemento con clase calc-input
    const inputs = form.querySelectorAll(".calc-input, input, select");

    inputs.forEach(input => {
        input.addEventListener("input", ejecutarCalculo);
        input.addEventListener("change", ejecutarCalculo);
    });

    function ejecutarCalculo() {
        const formData = new FormData(form);

        fetch('/Home/Calcular', {
            method: 'POST',
            body: formData
        })
        .then(response => {
            if (!response.ok) throw new Error("Error en la respuesta del servidor");
            return response.json();
        })
        .then(data => {
            // 1. Actualizar Resistencia y Pesos en la Matriz Superior
            if (document.getElementById("resUrdimbreRes")) 
                document.getElementById("resUrdimbreRes").innerText = data.resistenciaUrdimbre.toFixed(1) + " KgF";
            if (document.getElementById("resUrdimbrePeso")) 
                document.getElementById("resUrdimbrePeso").innerText = data.pesoUrdimbre.toFixed(1) + " gr";
            if (document.getElementById("resUrdimbrePorc")) 
                document.getElementById("resUrdimbrePorc").innerText = data.porcentajeUrdimbre.toFixed(1) + " %";

            if (document.getElementById("resTramaRes")) 
                document.getElementById("resTramaRes").innerText = data.resistenciaTrama.toFixed(1) + " KgF";
            if (document.getElementById("resTramaPeso")) 
                document.getElementById("resTramaPeso").innerText = data.pesoTrama.toFixed(1) + " gr";
            if (document.getElementById("resTramaPorc")) 
                document.getElementById("resTramaPorc").innerText = data.porcentajeTrama.toFixed(1) + " %";

            if (document.getElementById("resUrdRefRes")) 
                document.getElementById("resUrdRefRes").innerText = data.urdimbreRefuerzoResistencia.toFixed(1) + " KgF";
            
            if (document.getElementById("resPesoMetro")) 
                document.getElementById("resPesoMetro").innerText = data.pesoMetroLineal.toFixed(1) + " gml";

            // 2. Actualizar Tabla Inferior de Resultados
            if (document.getElementById("lblTipoProductoRes")) 
                document.getElementById("lblTipoProductoRes").innerText = data.tipoProducto;
            if (document.getElementById("resMedida")) 
                document.getElementById("resMedida").innerText = data.ancho + " x " + data.corte;
            
            // Actualizar textos dinámicos de tejido y denier en resultados
            const txtTejidoRes = document.getElementById("txtTejidoRes");
            if (txtTejidoRes) txtTejidoRes.innerText = data.urdimbreTejido + " x " + data.tramaTejido;

            const txtDenierRes = document.getElementById("txtDenierRes");
            if (txtDenierRes) txtDenierRes.innerText = data.urdimbreDenier + " x " + data.tramaDenier;

            if (document.getElementById("resUrdimbreRes2")) 
                document.getElementById("resUrdimbreRes2").innerText = data.resistenciaUrdimbre.toFixed(1);
            if (document.getElementById("resUrdimbrePorc2")) 
                document.getElementById("resUrdimbrePorc2").innerText = data.porcentajeUrdimbre.toFixed(0);

            if (document.getElementById("resTramaRes2")) 
                document.getElementById("resTramaRes2").innerText = data.resistenciaTrama.toFixed(1);
            if (document.getElementById("resTramaPorc2")) 
                document.getElementById("resTramaPorc2").innerText = data.porcentajeTrama.toFixed(0);

            if (document.getElementById("resUrdRefRes2")) 
                document.getElementById("resUrdRefRes2").innerText = data.urdimbreRefuerzoResistencia.toFixed(1);

            // Valores de Peso final (GM2, PP+LAM, GMP, GML)
            if (document.getElementById("resPesoBase")) 
                document.getElementById("resPesoBase").innerText = data.pesoTejidoBase.toFixed(1);
            if (document.getElementById("resPesoLaminado")) 
                document.getElementById("resPesoLaminado").innerText = data.pesoConLaminado.toFixed(1);
            if (document.getElementById("resPesoRefuerzo")) 
                document.getElementById("resPesoRefuerzo").innerText = data.pesoConRefuerzo.toFixed(1);
            if (document.getElementById("resPesoMetro2")) 
                document.getElementById("resPesoMetro2").innerText = data.pesoMetroLineal.toFixed(1);
        })
        .catch(error => console.error("Error en cálculo en tiempo real:", error));
    }
});
