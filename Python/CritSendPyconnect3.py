# -*- coding: utf-8 -*-
"""
Connector to CritSend's SOAP API v.3
"""

# Python
import hmac
import hashlib
import datetime
import random
import socket

import urllib2
import logging

# Suds
from suds import WebFault
from suds.client import Client
from suds.transport import TransportError
from suds.transport.http import HttpTransport

logging.basicConfig(filename='mxmconnect2.log',level=logging.INFO)
logging.getLogger('suds.client').setLevel(logging.DEBUG)
#logging.getLogger('suds.metrics').setLevel(logging.DEBUG)
#logging.getLogger('suds').setLevel(logging.DEBUG)
mxm_logger = logging.getLogger()


class MxmException(Exception):
    pass


class MxmConnect():
    """
    Client configuration
    """
    encoding        = 'UTF-8'
    wsdl            = '/api_2.php?wsdl'
    default_host    = 'http://mail1.messaging-master.com'
    debug           = False
    fast            = False
    client          = None
    max_retries     = 3

    def __init__(self, user, key, fast = False):
        self.user = user
        self.key = key
        self.fast = fast
        self.fast_host = ['http://mail1.messaging-master.com',
                           'http://mail5.messaging-master.com']
        self.hosts = [
        'http://sender31.critsend.com',
        'http://sender32.critsend.com'
                        ]

        self.client = self._select_client(False)



    def _select_client(self, internal = False):
        if self.fast == False and internal == True:
            # to update
            which = random.randint(0, len(self.hosts) - 1)
            client = self._makeClient(self.hosts[which])
        elif self.fast == True and internal == True:
            which = random.randint(0, len(self.fast_host) - 1)
            client = self._makeClient(self.fast_host[which])
        else:
            client = self._makeClient(self.default_host)


        # Exceptional case so we should isolate it
        if client == False:
            # We try all the hosts
            while client == False and len(self.hosts) > 0:
                which = random.randint(0, len(self.hosts) - 1)
                client = self._makeClient(self.hosts[which])
                if client == False:
                    self.hosts.remove(self.hosts[which])
            # If we did not find anything suitable
            if client == False:
                mxm_logger.info("No host found")
                raise MxmException("No host found")
        return client



    def createTag(self, tag):
        """
        This method creates a tag. It is idempotent.
        Returns boolean true if the tag was created; false otherwise
        """
        if len(tag) > 40 or len(tag.split()) > 1:
            mxm_logger.info("Connector: Invalid tag")
            raise MxmException("Connector: Invalid tag")
        auth = self._getAuth()
        return self.client.service.createTag(auth, tag)


    def deleteTag(self, tag):
        """
        This method deletes a tag and return a boolean.
        Returns boolean true if the tag was deleted;
        false otherwise (especially if it is not here)
        """
        if len(tag) > 40 or len(tag.split()) > 1:
            mxm_logger.info("Connector: Invalid tag")
            raise MxmException("Connector: Invalid tag")
        auth = self._getAuth()
        return self.client.service.deleteTag(auth, tag)


    def listAllTags(self):
        """
        This method returns a list containing all available tags.
        """
        auth = self._getAuth()
        try:
            result = self.client.service.listAllTags(auth)
        except Exception, e:
            mxm_logger.info("CritSend: Can't list all tags: %s" % str(e))
            raise MxmException("CritSend: Can't list all tags: %s" % str(e))
        if len(result) == 0:
            return []
        return [str(tag) for tag in result.Tag]


    def isTag(self, tag):
        """
        This method checks whether a tag exists or not. It is idempotent.
        Returns boolean true if the tag exists; false otherwise.
        """
        if len(tag) > 40 or len(tag.split()) > 1:
            mxm_logger.info("Connector: Invalid tag")
            raise MxmException("Connector: Invalid tag")
        auth = self._getAuth()
        return self.client.service.isTag(auth, tag)


    def sendCampaign(self, content, param_campaign, subscribers):
        """
        param `content` is a dictionary with keys 'subject', 'html' and 'text'.
        Their values should be strings. If you pass only html or text it will work.
        Example:
            test_content = {'subject': 'my subject',
                            'html': "my html",
                            'text': 'my text'}

        param `param_campaign` is a dictionary which follows the pattern:
            test_params =  {'tag': ['tag-1', 'tag-2'],
                            'mailfrom': 'nico@mxmaster.net',
                            'mailfrom_friendly': 'Test Nico',
                            'replyto': 'ntoper@gmail.com',
                            'replyto_filtered': 'true'}
        'replyto_filtered' key is not used yet.

        param `subscribers` is a list of dictionaries like:
            test_database = [{'email': 'georgioskollias@gmail.com',
                              'field1': 'George',
                              'field2': 'Kollias',
                              'field3': 'and',
                              'field4': 'so',
                              'field5': 'on'}]
        you can insert until 15 fields.
        """
        # Create content
        client = self._select_client(True)
        content_obj = client.factory.create('Content')
        for con in ['subject', 'html', 'text']:
            if con in content:
                setattr(content_obj, con, content[con])
            else:
                setattr(content_obj, con, '')

        # Create params
        campaignparameters = client.factory.create('CampaignParameters')
        arraytag = client.factory.create('ArrayTag')
        for t in param_campaign['tag']:
            arraytag.Tag.append(t)
        campaignparameters.tag = arraytag
        for p in ['mailfrom', 'mailfrom_friendly', 'replyto', 'replyto_filtered']:
            if p in param_campaign:
                setattr(campaignparameters, p, param_campaign[p])
            else:
                setattr(campaignparameters, p, '')

        # Create subscribers
        arrayemail = client.factory.create('ArrayEmail')
        for subscriber in subscribers:
            email = client.factory.create('Email')
            email.email = subscriber['email']
            for j in range(1,16):
                field_name = ''.join(['field', str(j)])
                if field_name in subscriber:
                    setattr(email, field_name, subscriber[field_name])
                else:
                    setattr(email, field_name, '')
            arrayemail.Email.append(email)

        auth = self._getAuth()
        try:
            res = client.service.sendCampaign(auth, arrayemail,\
                    campaignparameters, content_obj)
            return res
        except urllib2.URLError, e:
            mxm_logger.info("CritSend: fault tolerance activated")
            mxm_logger.info(str(e))
            return self.sendCampaign(content, param_campaign, subscribers)
        except TransportError, e:
            mxm_logger.info("CritSend: fault tolerance activated")
            mxm_logger.info(e)
            mxm_logger.info(e.fault)
            return self.sendCampaign(content, param_campaign, subscribers)

        except WebFault, e:
            mxm_logger.info("CritSend: fault tolerance activated")
            mxm_logger.info(e)
            mxm_logger.info(e.fault)
            return self.sendCampaign(content, param_campaign, subscribers)


    def _getAuth(self):
        timestamp = datetime.datetime.utcnow().strftime("%Y-%m-%dT%H:%M:%SZ")
        msg = "http://mxmaster.net/campaign/0.1#doCampaign" + self.user + \
                timestamp
        signature = hmac.new(self.key, msg, hashlib.sha256)
        return {'user': self.user,
                'timestamp': timestamp,
                'signature': signature.hexdigest()}


    def _makeClient(self, host):

        try:
            timeout = 2400
            socket.setdefaulttimeout(timeout)
            client = Client(host + self.wsdl)
            auth = self._getAuth()
            res = client.service.isTag(auth, 'default')

        except TransportError, e:
            client = False
        except WebFault, e:
            client = False
        except Exception, e:
            client = False
        return client

