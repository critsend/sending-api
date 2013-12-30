<?php
/**
* Connector
*
* 
* Version 3 support@critsend.com
*/


class MxmException extends Exception {
	public function __construct($err = "") {
		parent::__construct($err);	
	}
}

class NotImplementedException extends Exception {
	public function __construct($err = "") {
		parent::__construct($err);	
	}
}

/*
 * Each public method can raise MxMException (it contains a string describing the error). Check also for a true or no answer/false (signals error or crash)
 */
class MxmConnect {
	/*
	* Client configuration
	*/
	
	//Change this to ISO-8859-1  for encoding
	private static $encoding = 'ISO-8859-1';
  
	private static $user = '';
    private static $key = '';

    
	private static $wsdl ='/api_2.php?wsdl';
	//Host for reporting :)
	private static $default_host = 'http://mail1.messaging-master.com';
	private static $fast_host = array('http://smtp.critsend.com', 'http://smtp.critsend.com');
	private static $hosts = array('http://smtp.critsend.com', 
	                        'http://smtp.critsend.com',
	                        'http://smtp.critsend.com',
	                        'http://smtp.critsend.com',
	                        'http://smtp.critsend.com',
	                        );

	//Put in to true to activate debug mode
	private static $debug = false;
	private $fast = False;
    static private $instance = null;
    
    public static function getInstance($var) {
		if (self::$instance == null){
			self::$instance = new self($var);
		}
		return self::$instance;
	}

	/*
	* Public Method
	*/
	public function __construct($fast = False, $internal = False){
	    $this->fast = $fast;
	    
		if ($fast == False && $internal == True) {
		    //to update
            $idx = mt_rand(0, count(self::$hosts) - 1);
			$client = @self::makeClient(self::$hosts[$idx]);
		}
		elseif ($fast == True && $internal == True) {
		    $idx = mt_rand(0, 1);
			$client = @self::makeClient(self::$fast_host[$idx]);
		}	
		else {
		    $client = @self::makeClient(self::$default_host);
        }
		//Exceptional case so we should isolate it
		if ($client === False) {
			$hosts = self::$hosts;
		
	        //We try all the hosts.
		    while ($client === False and count($hosts) > 0) {
			    $new_host = array_pop($hosts);
			    $client = @self::makeClient($new_host);
		}
		
		//If we did not find anything suitable
		if ($client === False) {
			throw new MxmException("no host found");
		}
	}
	
	$this->client = $client;
}
	

	/**
	 *  This method creates a tag. It is idempotent
	 *
	 * @param tag is a string of less than 40 characters and without spaces or non-ASCII characters
	 * @return boolean true if the tag was created; false otherwise
	 * @throw MxmException with soap error message
	 */
	public function createTag($tag) {
		if (strlen($tag) > 40 || strpos($tag, ' ')) {
			throw new MxmException("invalid tag (connector answered)");
		}
		$auth = $this->getAuth();
		return $this->client->createTag($auth ,$tag);		
	}

	/**
	 *  This method deletes a tag.
	 *
	 * @param tag is a string of less than 40 characters and without spaces or non-ASCII characters
	 * @return boolean true if the tag was deleted; false otherwise (especially if it is not heree)
	 * @throw MxmException with soap error message
	 */
	public function deleteTag($tag) {
		if (strlen($tag) > 40 || strpos($tag, ' ')) {
			throw new MxmException("invalid tag (connector answered)");
		}
		$auth = $this->getAuth();
		return $this->client->deleteTag($auth ,$tag);
	}
	
	/**
	 *  This method lists all existing tags.
	 *
	 * @param tag is a string of less than 40 characters and without spaces or non-ASCII characters
	 * @return array containing the tags.
	 * @throw MxmException with soap error message
	 */
	public function listAllTags() {
		$auth = $this->getAuth();
		$res = $this->client->listAllTags($auth);
		//Traitement du r�sultat pour avoir toujours un tableau
		if (!(empty($res->Tag))) {
			if (count($res->Tag == 1)) {
				return array($res->Tag);
			}
			return $res->Tag;
		}
		else
			return array();
	}

	/**
	 *  This method checks whether a tag exists or not. It is idempotent
	 *
	 * @param tag is a string of less than 40 characters and without spaces or non-ASCII characters
	 * @return boolean true if the tag exists; false otherwise
	 * @throw MxmException with soap error message
	 */
	public function isTag($tag) {
		if (strlen($tag) > 40 || strpos($tag, ' ')) {
			throw new MxmException("invalid tag (connector answered)");
		}
		$auth = $this->getAuth();
		return $this->client->isTag($auth ,$tag);
	}



	/**
	 * 
	 * Be careful, variables are passed by reference and I update them. This is of couse for performance reasons (you can pass up to 5 000 email addresses)
	 * 
	 * 
	 * @param array param_email is an associative array following the pattern array('tag'=>array('mon_tag_1', 'mon_tag_2'), 'mailfrom'=> 'nico@mxmaster.net',
	 * 'mailfrom_friendly'=>'Nico', 'replyto'=>'ntoper@gmail.com', 'replyto_filtered'=> 'true'); 
	 *
	 * replyto_filtered'=> 'true' is not used yet.
	 *
	 * @param array content  is an associative array following the pattern  array('subject'=> 'my subject', 'html'=> "my html", 'text' =>'my texte');
	 * If you pass only html or text it will work. In this case set html=> '' for instance.
	 *
	 * @param array database is an array array of array('email'=>'ntoper@gmail.com', 'field1'=>'Nicolas', 'field2'=>'Toper',
	 * 'field3'=>'and','field5'=>'so', 'field5'=>'on').
	 *
	 * You only need to set the defined variable. For instance, if I use only two fields then I would pass for instance:
	 * array('email'=>'ntoper@gmail.com', 'field1'=>'Nico', 'field2'=>'homme')
	 *
	 *
	 * For instance,
	 * $database = array( 
	 *				array('email'=>'ntoper@gmail.com', 'field1'=>'béàè'),
	 *				array('email'=>'nico@mxmaster.net','field2'=>'a'),
	 *				array('email'=>'nt@mxmaster.net','field2'=>'a')
	 *				);
	 *
     * @return boolean true if the delivery is scheduled; false otherwise
	 *
	 * @throw MxmException with soap error message
     *
	 */
	public function sendCampaign(&$content, &$param_campaign, &$subscribers) {
			//Besoin?
			//canonicanization
			
			if ($this->fast == True) {
			    $mxm = new MxmConnect(True, True);
			    
			}
			else {
			    $mxm = new MxmConnect(False, True);
		    }
		    
			$nbr = count($subscribers);
			for($i = 0; $i < $nbr; $i++) {
				for ($j = 1; $j < 16; $j++) {					
					if (!array_key_exists('field'.$j, $subscribers[$i])){
						$subscribers[$i]['field'.$j] = '';
					}
				}				
			}

			$auth = $this->getAuth();
			try {
				return $mxm->client->sendCampaign($auth, $subscribers, $param_campaign, $content);
			} catch (SoapFault $e) {
				throw new MxmException($e->getMessage());
				
			}			
	}
	
	
	public function sendEmail(&$content, &$param_campaign, &$subscribers) {
	    throw new NotImplementedException();
	}

	/*
	* Private methods
	*/

	protected $client = null;

	
	private function getAuth() {
		//TODO Memory leak here of 32 octets: use date instead as in comment below
		$timestamp = gmstrftime("%Y-%m-%dT%H:%M:%SZ", time());
		//$timestamp = date(DATE_W3C);
		return array('user'=> self::$user, 'timestamp'=> $timestamp, 'signature'=>hash_hmac("sha256", "http://mxmaster.net/campaign/0.1#doCampaign".self::$user.$timestamp, self::$key));
	}
	
	
	private static function makeClient($host) {
		try {
			$client = new SoapClient($host.self::$wsdl, 
			  		array('compression' => SOAP_COMPRESSION_ACCEPT | SOAP_COMPRESSION_GZIP, 'trace'=> False,'encoding'=> self::$encoding));
		} catch (SoapFault $e) {
			return False;
		}
		return $client;
	}

}

?>
