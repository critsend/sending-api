/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package net.mxm.connector.test;

import net.mxm.connector.*;
import net.mxm.connector.test.MxmTest;

/**
 *
 * @author nico
 */
public class MxmSingleTest {
    public static void main(String args[]) throws MxmConnectException, MxmConnectConfigurationException {
           MxmTest m = new MxmTest();
           m.sendEmail();
           //m.testOthers();
        
           
        
       /* for (int i = 0; i < 100; i++) {
            System.gc();
        }*/
        System.out.println(Runtime.getRuntime().totalMemory() - Runtime.getRuntime().freeMemory());
        
    }

}
