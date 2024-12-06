// See https://aka.ms/new-console-template for more information
using DCI.Social.Fortification;
using DCI.Social.Spelunking;

var sample = new SampleForTransport(
    SampleLong: 232320L,
    SampleNullableLong: 0L,
    SampleString: "fjf0320q2ld.vc2'2322",
    SampleNullableString: "",
    SampleDate: DateTime.Now,
    SampleNullableDate: DateTime.MinValue
   );


var senderSideEncStream = new SocialEncryptedStream();
var receiverSideEncStream = new SocialEncryptedStream(senderSideEncStream.Key);

var forTransport = senderSideEncStream.EncryptForTransport(sample);
var decrypted = receiverSideEncStream.DeserializeFromTransport<SampleForTransport>(forTransport);

var tessa = "";
