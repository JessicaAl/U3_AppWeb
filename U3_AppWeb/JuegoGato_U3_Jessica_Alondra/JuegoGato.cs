using GalaSoft.MvvmLight.Command;
using JuegoGato_U3_Jessica_Alondra.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
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
        public bool VentanaPrincipalVisible { get; set; } = true;

        public byte PuntosJugador1 { get; set; }
        public byte PuntosJugador2 { get; set; }
        public string MovimientoJugador1 { get; set; }
        public Movimiento? SeleccionJugador1 { get; set; }
        public Movimiento? SeleccionJugador2 { get; set; }
        public string Mensaje { get; set; }

        public ICommand SeleccionarMovimientoCommand { get; set; }
        public ICommand ConfirmarCommand { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        HttpListener servidor;
        ClientWebSocket cliente;
        Dispatcher currentDispatcher;
        public JuegoGato()
        {
            currentDispatcher = Dispatcher.CurrentDispatcher;
            ConfirmarCommand = new RelayCommand<bool>(IniciarPartida);
        }

        private void Lobby_Closing(object sender, CancelEventArgs e)
        {
            VentanaPrincipalVisible = true;
            Actualizar("VentanaPrincipalVisible");
            if (servidor != null)
            {
                servidor.Stop();
                servidor = null;
            }
        }
        Lobby lobby;
        private async void IniciarPartida(bool partida)
        {
            try
            {
                VentanaPrincipalVisible = false;
                lobby = new Lobby();
                lobby.Closing += Lobby_Closing;
                lobby.DataContext = this;
                lobby.Show();
                Actualizar();
                if (partida == true)
                {
                    CrearPartida();
                }
                else
                {
                    Mensaje = "Intentando conectar con el servidor en " + IP;
                    Actualizar("Mensaje");
                    await ConectarPartida();
                }
            }
            catch (Exception ex)
            {
                Mensaje = ex.Message;
                Actualizar();
            }
        }
        public void CrearPartida()
        {
            servidor = new HttpListener();
            servidor.Prefixes.Add("http://*:1000/gato/");
            servidor.Start();
            servidor.BeginGetContext(OnContext, null);
            Mensaje = "Esperando a conectar con un contrincante...";
            Actualizar();
        }
        public async Task ConectarPartida()
        {
            cliente = new ClientWebSocket();
            await cliente.ConnectAsync(new Uri($"ws://{IP}:1000/gato/"), CancellationToken.None);

        }
        private void OnContext(IAsyncResult ar)
        {
            //
        }
        void Actualizar(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}