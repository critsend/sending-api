/*
 * This class is just here for demo purposes.
 */
package net.mxm.connector.test;

import java.util.logging.Level;
import java.util.logging.Logger;

import net.mxm.connector.ArrayEmail;
import net.mxm.connector.CampaignParameters;
import net.mxm.connector.Content;
import net.mxm.connector.Email;
import net.mxm.connector.MxmConnect;
import net.mxm.connector.MxmConnectConfigurationException;
import net.mxm.connector.MxmConnectException;

/**
 * @author nico toper
 * @date 2010-10-20
 */
public class MxmTest extends Thread {
	public MxmConnect mxm = null;
    
	public int res[] = new int[20];
    
	public MxmTest() throws MxmConnectException {
		//mxm = new MxmConnect();
		for (int i = 0; i < res.length; i++) {
			res[i] = 0;
		}
        
	}
    
	@Override
	public void run() {
        
		for (int i = 0; i < 2000; i++) {
			try {
				try {
					long fStart = System.currentTimeMillis();
                    
					this.sendEmail();
					//System.out.println("Time for this mail: " + ( System.currentTimeMillis() - fStart) );
                    
				} catch (MxmConnectException ex) {
					Logger.getLogger(MxmTest.class.getName()).log(Level.SEVERE, null, ex);
				}
			} catch (MxmConnectConfigurationException ex) {
				Logger.getLogger(MxmTest.class.getName()).log(Level.SEVERE, null, ex);
			}
		}
        
	}
    
	void testOthers() throws MxmConnectException, MxmConnectConfigurationException {
		MxmConnect mxm2 = new MxmConnect("", "");
		//  mxm2.createTag("test");
		// mxm2.createTag("test2");
		String[] t = mxm2.listAllTags();
        
		for (int i = 0; i < t.length; i++) {
			System.out.println(t[i]);
            
		}
        
		// System.out.println(mxm2.deleteTag("test2"));
	}
    
	private String makeRandom() {
		Double r = new Double(Math.random() * 1000000);
		return r.toString(r);
	}
    
	protected void sendEmail() throws MxmConnectConfigurationException, MxmConnectException {
        
		MxmConnect mxm2 = new MxmConnect("", "");
		//MxmConnect mxm2 = mxm ;
		// mxm2.setFastDelivery(true);
		CampaignParameters cp = new CampaignParameters();
		cp.setMailFrom("test@test.com");
		cp.setMailFromFriendly("FriendlyName");
		cp.setReplyTo(this.makeRandom());
		cp.setReplyToFiltered(false);
		String tags[] = { "test" };
		cp.setTags(tags);
        
		Content c = new Content();
		//c.setSubject(this.makeRandom());
		//c.setText(this.makeRandom());
        
		c.setSubject("Your Subject");
		c.setText("test");
        
		StringBuffer sb = new StringBuffer();
        
		String r = this.makeRandom();
		for (int i = 0; i < 2000; i++) {
            
			sb.append(r);
		}
        
		c.setHtml(sb.toString());
        
		Email e = new Email();
		e.setEmail("test@test.com");
        
		ArrayEmail ae = new ArrayEmail();
		ae.addEmail(e);
        
		String host = mxm2.getHost();
		System.out.println(host);
        
        
		System.out.println(mxm2.sendCampaign(c, cp, ae));
		//System.gc();
        
	}
    
	//This is the entry point for test.
	public static void main(String[] args) throws MxmConnectException, MxmConnectConfigurationException {
		//HeapMonitor h = new HeapMonitor();
		//h.start();
		for (int i = 0; i < 1; i++) {
			MxmTest mxm = new MxmTest();
			for (int j = 0; j < 100000; j++) {
				mxm.sendEmail();
				System.out.println("ok");
				for (int x = 0; x < mxm.res.length; x++) {
					System.out.println(x + ": " + mxm.res[x]);
				}
                
			}
		}
        
	}
    
}
