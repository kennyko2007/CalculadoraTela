namespace CalculadoraTela.ViewModels
{
    public class CalculadoraTelaVM
    {
        // ==========================================
        // 1. ENTRADAS: PARÁMETROS DE TEJIDO Y HILO
        // ==========================================
        
        /// <summary> Densidad del tejido en Urdimbre (ej. 13) </summary>
        public double UrdimbreTejido { get; set; } = 13.0;

        /// <summary> Densidad del tejido en Trama (ej. 11) </summary>
        public double TramaTejido { get; set; } = 11.0;

        /// <summary> Ancho o medida de la cinta en Urdimbre </summary>
        public double CintaUrdimbre { get; set; } = 2.0;

        /// <summary> Ancho o medida de la cinta en Trama </summary>
        public double CintaTrama { get; set; } = 4.0;

        /// <summary> Titulaje Denier en Urdimbre (ej. 1200) </summary>
        public double UrdimbreDenier { get; set; } = 1200;

        /// <summary> Titulaje Denier en Trama (ej. 1500) </summary>
        public double TramaDenier { get; set; } = 1500;

        // ==========================================
        // 2. ENTRADAS: DIMENSIONES Y PROCESOS
        // ==========================================

        /// <summary> Ancho del tejido en cm (ej. 90.0) </summary>
        public double Ancho { get; set; } = 90.0;

        /// <summary> Medida del corte en cm </summary>
        public double Corte { get; set; } = 0.0;

        /// <summary> Medida de la costura en cm </summary>
        public double Costura { get; set; } = 0.0;

        /// <summary> Valor de laminado (0 si no aplica) </summary>
        public double Laminado { get; set; } = 0.0;

        /// <summary> Factor multiplicador de refuerzo (G5 en Excel, por defecto 1.07) </summary>
        public double AnchoRefuerzoFactor { get; set; } = 1.07;

        /// <summary> Lado / Caras del tejido (E8 en Excel, por defecto 2.0) </summary>
        public double Lado { get; set; } = 2.0;

        // ==========================================
        // 3. ENTRADAS: MÁQUINA Y PRODUCCIÓN
        // ==========================================

        /// <summary> Número o código de máquina (ej. 18) </summary>
        public double MaquinaNumero { get; set; } = 18;

        /// <summary> Medida del engranaje en máquina (ej. 10.5) </summary>
        public double Engranaje { get; set; } = 10.5;

        /// <summary> Horas proyectadas de trabajo (ej. 10) </summary>
        public double Horas { get; set; } = 10.0;

        // ==========================================
        // 4. SALIDAS CALCULADAS (PROCESADAS EN C#)
        // ==========================================

        /// <summary> Peso unitario Urdimbre </summary>
        public double PesoUrdimbre { get; set; }

        /// <summary> Peso unitario Trama </summary>
        public double PesoTrama { get; set; }

        /// <summary> Resistencia Urdimbre </summary>
        public double ResistenciaUrdimbre { get; set; }

        /// <summary> Resistencia Trama </summary>
        public double ResistenciaTrama { get; set; }

        /// <summary> Resistencia del refuerzo de Urdimbre </summary>
        public double ResistenciaUrdimbreRefuerzo { get; set; }

        /// <summary> Porcentaje de participación Urdimbre (%) </summary>
        public double PorcentajeUrdimbre { get; set; }

        /// <summary> Porcentaje de participación Trama (%) </summary>
        public double PorcentajeTrama { get; set; }

        /// <summary> Cantidad de cintas (Calculado con la función EVEN) </summary>
        public int Cantidad { get; set; }

        /// <summary> Peso base del tejido en gm2 </summary>
        public double PesoTejidoBase { get; set; }

        /// <summary> Peso con laminado en gm2 (H4 en Excel) </summary>
        public double PesoConLaminado { get; set; }

        /// <summary> Peso con refuerzo en gmp (H5 en Excel) </summary>
        public double PesoConRefuerzo { get; set; }

        /// <summary> Peso por metro lineal en gml (H7 en Excel) </summary>
        public double PesoMetroLineal { get; set; }

        /// <summary> Peso en gramos por bolsa gr/Bol (H9 en Excel) </summary>
        public double PesoPorBolsa { get; set; }

        /// <summary> Producción total estimada en metros </summary>
        public double ProduccionEstimada { get; set; }

        /// <summary> Cadena resumen de la Ficha Técnica (A11 en Excel) </summary>
        public string ResumenFicha { get; set; } = string.Empty;
    }
}
