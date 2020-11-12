using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace JuegoGato_U3_Jessica_Alondra
{
    public class JuegoGato : INotifyPropertyChanged
    {
        public enum Movimiento { A1, A2, A3, B1, B2, B3, C1, C2, C3 }
        public string NombreJugador1 { get; set; } = "Jugador";
        public string NombreJugador2 { get; set; }
        public string IP { get; set; }

        public byte PuntosJugador1 { get; set; }
        public byte PuntosJugador2 { get; set; }
        public string MovimientoJugador1 { get; set; }
        public Movimiento? SeleccionJugador1 { get; set; }
        public Movimiento? SeleccionJugador2 { get; set; }
        public string Mensaje { get; set; }

        public ICommand SeleccionarMovimientoCommand { get; set; }
        public ICommand IniciarCommand { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        HttpListener servidor;
        ClientWebSocket cliente;
        Dispatcher currentDispatcher;
        public JuegoGato()
        {
            currentDispatcher = Dispatcher.CurrentDispatcher;
            IniciarCommand = new RelayCommand<bool>(IniciarPartida);
        }

        private void IniciarPartida(bool partida)
        {
            if (partida == true)
            {
                CrearPartida();
            }
            else
            {
                ConectarPartida();
            }
        }

        public void CrearPartida()
        {
            servidor = new HttpListener();
            servidor.Prefixes.Add("http://*:1000/gato");
            servidor.Start();
            servidor.BeginGetContext(OnContext, null);
            Mensaje = "Esperando a conectar con un contrincante...";
            Actualizar();
        }
        public void ConectarPartida()
        {

        }
        private void OnContext(IAsyncResult ar)
        {
            throw new NotImplementedException();
        }
        void Actualizar(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}