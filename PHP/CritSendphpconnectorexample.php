$content = array('subject'=> 'My subject', 'html'=> 'my html' , 'text' =>'my test');

$param = array('tag'=>array('invoice1'), 'mailfrom'=> 'ntoper@critsend.com', 'mailfrom_friendly'=> 'Nicolas Toper', 'replyto'=>'ntoper@critsend.com', 'replyto_filtered'=> 'true');

$emails[0] = array('email'=>'happy@customer.com', 'field1'=> 'test');

}
   try {
      echo $j;
      print_r($mxm->sendCampaign($content, $param, $emails));
   } catch (MxmException $e) {
   echo $e->getMessage();
}