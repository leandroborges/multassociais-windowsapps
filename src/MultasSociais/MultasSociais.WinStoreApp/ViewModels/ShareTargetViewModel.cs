﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using MultasSociais.Lib;
using MultasSociais.Lib.Models;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace MultasSociais.WinStoreApp.ViewModels
{
    public class ShareTargetViewModel : ViewModelBase
    {
        private HttpUpload.FileInfo fileInfo;
        
        public ShareTargetViewModel(INavigationService navigationService, ITalao talao) : base(navigationService, talao) {}

        protected async override void OnInitialize()
        {
            base.OnInitialize();

            await TentarObterThumbnail();

            await ObterImagem();
        }

        private async Task TentarObterThumbnail()
        {
            var shareProperties = ShareOperation.Data.Properties;
            if (shareProperties.Thumbnail == null) return;
            var stream = await shareProperties.Thumbnail.OpenReadAsync();
            await ExibirImagem(stream);
        }

        private async Task ObterImagem()
        {
            IRandomAccessStream streamCloned = null;
            if (ShareOperation.Data.Contains(StandardDataFormats.Bitmap))
            {
                var sharedBitmapRandomAccessStreamReference = await ShareOperation.Data.GetBitmapAsync();
                var stream = await sharedBitmapRandomAccessStreamReference.OpenReadAsync();
                streamCloned = stream.CloneStream();
                fileInfo = new HttpUpload.FileInfo
                               {
                                   FileName = ObterNomeAleatorioDeArquivo(stream.ContentType),
                                   ContentType = stream.ContentType,
                                   Buffer = await stream.GetByteFromFileAsync(),
                                   ParamName = "multa[foto]"
                               };
            }
            else if (ShareOperation.Data.Contains(StandardDataFormats.StorageItems))
            {
                var storageItems = await ShareOperation.Data.GetStorageItemsAsync();
                var storageFile = (StorageFile) storageItems.FirstOrDefault();
                if (storageFile == null) return;
                var stream = await storageFile.OpenReadAsync();
                streamCloned = stream.CloneStream();
                fileInfo = new HttpUpload.FileInfo
                               {
                                   FileName = storageFile.Name,
                                   ContentType = stream.ContentType,
                                   Buffer = await stream.GetByteFromFileAsync(),
                                   ParamName = "multa[foto]"
                               };
            }
            await ExibirImagem(streamCloned);
        }

        private async Task ExibirImagem(IRandomAccessStream stream)
        {
            if (Image != null) return;
            Image = new BitmapImage();
            await Image.SetSourceAsync(stream);
            ShowImage = true;
        }


        private string ObterNomeAleatorioDeArquivo(string contentType)
        {
            var nomeAleatorio = "arq" + new Random().Next(1000000000, int.MaxValue).ToString();
            var ext = contentType.Split('/')[1];
            var nome = string.Format("{0}.{1}", nomeAleatorio, ext);
            return nome;
        }

        public bool CanShare
        { 
            get
            {
                return descricaoIsValid && videoUrlIsValid && dataOcorrenciaIsValid && !sharing;
            }
        }

        public async Task Share()
        {
            Sharing = true;
            ShareOperation.ReportStarted();
            var multa = new CriarMultaNova
                            {
                                Descricao = descricao,
                                Placa = placa,
                                VideoUrl = videoUrl
                            };
            multa.SetaDataOcorrencia(dataOcorrencia);
            try
            {
                MultadoComSucesso = await talao.MultarAsync(multa, fileInfo);
                if (MultadoComSucesso)
                {
                    Sharing = false;
                    await Task.Delay(1000);
                    ShareOperation.ReportCompleted();
                }
                else
                {
                    ShareOperation.ReportError("Não foi possível multar, favor tentar mais tarde.");
                }
            }
            catch (Exception ex)
            {
                ShareOperation.ReportError("Não foi possível multar, ocorreu um erro, favor tentar mais tarde.\nErro:" + ex.Message);
            }

        }

        public static ShareOperation ShareOperation { get; set; }
        private DateTime dataOcorrencia = DateTime.Now;
        public DateTime DataOcorrencia { get { return dataOcorrencia; } set { dataOcorrencia = value; NotifyOfPropertyChange(); } }
        private string descricao;
        public string Descricao { get { return descricao; } set { descricao = value; NotifyOfPropertyChange(); } }
        private string placa;
        public string Placa { get { return placa; } set { placa = value; NotifyOfPropertyChange(); } }
        private string videoUrl;
        public string VideoUrl { get { return videoUrl; } set { videoUrl = value; NotifyOfPropertyChange(); } }
        private bool multadoComSucesso;
        public bool MultadoComSucesso { get { return multadoComSucesso; } set { multadoComSucesso = value; NotifyOfPropertyChange(); } }
        private bool sharing;
        public bool Sharing { get { return sharing; } set { sharing = value; NotifyOfPropertyChange(); NotifyOfPropertyChange("CanShare"); } }
        private bool showImage;
        public bool ShowImage { get { return showImage; } set { showImage = value; NotifyOfPropertyChange(); } }
        private BitmapImage image;
        public BitmapImage Image { get { return image; } set { image = value; NotifyOfPropertyChange(); } }
        private bool videoUrlIsValid = true;
        public bool VideoUrlIsValid { get { return videoUrlIsValid; } set { videoUrlIsValid = value; NotifyOfPropertyChange("CanShare"); } }
        private bool descricaoIsValid;
        public bool DescricaoIsValid { get { return descricaoIsValid; } set { descricaoIsValid = value; NotifyOfPropertyChange("CanShare"); } }
        private bool dataOcorrenciaIsValid = true;
        public bool DataOcorrenciaIsValid { get { return dataOcorrenciaIsValid; } set { dataOcorrenciaIsValid = value; NotifyOfPropertyChange("CanShare"); } }
    }
}